// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Deeplex.Saverwalter.WebAPI.Utils
{
    /// <summary>
    /// Die Dateien eines Objekts (<c>{entity}/{id}</c>): Auflisten, Lesen, Hochladen, Löschen,
    /// Umbenennen und das Jahr wechseln. Dateien sind je Objekt über ihren Namen adressiert; in
    /// welchem Jahresordner (oder flach, ohne Jahr) sie liegen, löst der Dienst auf. Ein Upload
    /// landet im gewählten Jahr (<see cref="FileLayout.NoYear"/>: ohne Jahr), ohne Angabe dort, wo
    /// eine gleichnamige Datei schon liegt, sonst im Jahr des Objekts (z. B. das Datum eines
    /// Zählerstands), sonst ohne Jahr.
    /// </summary>
    public class EntityFileService(HttpClient client, string baseUrl)
    {
        private readonly EntityFileStore _store = new(client, baseUrl);

        /// <param name="apiPath">Pfad der Liste (<c>/api/{entity}/{id}/files</c>), Grundlage der Schlüssel.</param>
        public async Task<List<WalterFile>> ListAsync(string entity, string id, string apiPath) =>
            [.. (await _store.ListAsync($"{entity}/{id}/"))
                .Where(f => !f.IsTrash)
                .GroupBy(f => f.FileName)
                .Select(g => g.OrderByDescending(f => f.LastModified).First())
                .Select(f => new WalterFile
                {
                    FileName = f.FileName,
                    Key = $"{apiPath}/{f.FileName}",
                    LastModified = f.LastModified.ToUnixTimeMilliseconds(),
                    Size = f.Size,
                    Year = FileLayout.YearOrNone(f),
                })];

        /// <summary>
        /// Die Dateien der ganzen Familie, je Mitglied ein Listenaufruf. Der Schlüssel jeder Datei
        /// ist die Adresse ihres Eigentümers; <see cref="WalterFile.Source"/> nennt ihn, außer beim
        /// Objekt der Seite selbst (<paramref name="selfRoute"/>, <paramref name="selfId"/>).
        /// </summary>
        public async Task<List<WalterFile>> ListFamilyAsync(FileFamily family, string selfRoute, string selfId)
        {
            var members = family.Members.Any(m => m.Route == selfRoute && m.Id == selfId)
                ? family.Members
                : [.. family.Members, new FileOwner(selfRoute, selfId, "")];

            var lists = await Task.WhenAll(members.Select(async member =>
            {
                var files = await ListAsync(member.Route, member.Id, $"/api/{member.Route}/{member.Id}/files");
                var own = member.Route == selfRoute && member.Id == selfId;
                foreach (var file in files) file.Source = own ? null : member.Label;
                return files;
            }));
            return [.. lists.SelectMany(files => files)];
        }

        /// <summary>Liest oder löscht (je nach Methode der Anfrage; Löschen legt den Papierkorb-Marker an).</summary>
        public async Task<IActionResult> ReadOrDeleteAsync(HttpRequest request, string entity, string id, string fileName)
        {
            var existing = await _store.FindAsync(entity, id, fileName);
            var key = existing?.Key ?? FileLayout.FlatKey(entity, id, fileName);
            return await FileHandling.RedirectToFileServer(request, client, _store.Url(key));
        }

        public async Task<IActionResult> UploadAsync(HttpRequest request, string entity, string id, string fileName, int? year, int? defaultYear = null)
        {
            var existing = await _store.FindAsync(entity, id, fileName);
            var targetYear = year
                ?? (existing == null ? (int?)null : FileLayout.YearOrNone(existing))
                ?? defaultYear
                ?? FileLayout.NoYear;
            var target = FileLayout.KeyFor(entity, id, targetYear, fileName);

            var response = await FileHandling.PutAsync(request, client, _store.Url(target));
            if (response.IsSuccessStatusCode)
            {
                // Eine gleichnamige Datei an anderer Stelle (anderes Jahr, flach) ist damit ersetzt.
                if (existing != null && existing.Key != target) await _store.DeleteAsync(existing.Key);
            }
            return await FileHandling.ToResultAsync(response);
        }

        public async Task<IActionResult> RenameAsync(HttpRequest request, string entity, string id, string oldName, string newName)
        {
            var existing = await _store.FindAsync(entity, id, oldName);
            var target = $"{(existing != null ? FileLayout.Folder(existing.Key) : $"{entity}/{id}")}/{newName}";

            var response = await FileHandling.PutAsync(request, client, _store.Url(target));
            if (response.IsSuccessStatusCode)
                await _store.DeleteAsync(existing?.Key ?? FileLayout.FlatKey(entity, id, oldName));
            return await FileHandling.ToResultAsync(response);
        }

        /// <summary>Verschiebt die Datei in den Ordner des Jahres; <see cref="FileLayout.NoYear"/> nimmt ihr das Jahr.</summary>
        public async Task<IActionResult> ChangeYearAsync(string entity, string id, string fileName, int year)
        {
            var existing = await _store.FindAsync(entity, id, fileName);
            if (existing == null) return new NotFoundResult();

            var target = FileLayout.KeyFor(entity, id, year, fileName);
            if (existing.Key == target) return new OkResult();
            return await _store.MoveAsync(existing.Key, target)
                ? new OkResult()
                : new StatusCodeResult(StatusCodes.Status502BadGateway);
        }

        /// <summary>
        /// Sortiert flach liegende Dateien unter dem Präfix (leer = alles) in den Ordner ihres
        /// Upload-Jahres. Das geschieht nur auf Anforderung (<c>POST api/files/sort</c>); Dateien ohne
        /// Jahr sind ein erlaubter Zustand. Wiederholbar: sortierte Dateien bleiben unberührt; eine Datei, deren Ziel schon
        /// existiert, wird nur dann entfernt, wenn beide gleich groß sind (Rest eines unterbrochenen Verschiebens).
        /// </summary>
        /// <returns>Die Zahl der sortierten Dateien.</returns>
        public async Task<int> SortAsync(string prefix = "")
        {
            var files = await _store.ListAsync(prefix);
            var byKey = files.ToDictionary(f => f.Key);

            var sorted = 0;
            foreach (var file in files.Where(FileLayout.IsUnsorted))
            {
                var target = FileLayout.SortedKey(file);
                if (byKey.TryGetValue(target, out var present))
                {
                    if (present.Size == file.Size) await _store.DeleteAsync(file.Key);
                    continue;
                }
                if (await _store.MoveAsync(file.Key, target)) sorted++;
            }
            return sorted;
        }
    }
}
