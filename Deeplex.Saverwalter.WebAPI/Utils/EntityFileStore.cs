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

using System.Globalization;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Linq;

namespace Deeplex.Saverwalter.WebAPI.Utils
{
    /// <summary>Ein Objekt der Dateiablage (S3) mit seinem Schlüssel und Upload-Datum.</summary>
    public sealed record StoredFile(string Key, DateTimeOffset LastModified, long Size)
    {
        private string[] Segments => Key.Split('/');
        public string FileName => Segments[^1];
        /// <summary>Marker der gelöschten Dateien: <c>…/trash/{name}</c>.</summary>
        public bool IsTrash => Segments.Length >= 2 && Segments[^2] == FileLayout.Trash;
    }

    /// <summary>
    /// Wo die Dateien eines Objekts liegen: <c>{entity}/{id}/{jahr}/{name}</c>. Eine Datei darf
    /// auch flach unter <c>{entity}/{id}/{name}</c> liegen; sie hat dann kein Jahr. Beides wird
    /// gelesen und gleich behandelt; sortiert wird nur auf Wunsch.
    /// </summary>
    public static class FileLayout
    {
        public const string Trash = "trash";

        /// <summary>Die persönlichen Dateien eines Nutzers (<c>user/{id}/…</c>) sind keine Objekt-Dateien.</summary>
        private const string UserFolder = "user";

        public static string Key(string entity, string id, int year, string fileName) =>
            $"{entity}/{id}/{year}/{fileName}";

        public static string FlatKey(string entity, string id, string fileName) =>
            $"{entity}/{id}/{fileName}";

        /// <summary>Das Jahr des Ordners einer sortierten Datei, sonst <c>null</c>.</summary>
        public static int? Year(StoredFile file)
        {
            var segments = file.Key.Split('/');
            return !file.IsTrash
                && segments.Length == 4
                && segments[0] != UserFolder
                && segments[2].Length == 4
                && int.TryParse(segments[2], NumberStyles.None, CultureInfo.InvariantCulture, out var year)
                    ? year
                    : null;
        }

        /// <summary>Das Jahr der Datei: der Ordner; 0 für eine Datei ohne Jahr (flach unter <c>{entity}/{id}/</c>).</summary>
        public static int YearOrNone(StoredFile file) => Year(file) ?? NoYear;

        /// <summary>Kennzeichnet eine Datei ohne Jahr.</summary>
        public const int NoYear = 0;

        /// <summary>Der Schlüssel im Jahresordner; mit <see cref="NoYear"/> flach, ohne Jahr.</summary>
        public static string KeyFor(string entity, string id, int year, string fileName) =>
            year == NoYear ? FlatKey(entity, id, fileName) : Key(entity, id, year, fileName);

        /// <summary>Flach unter <c>{entity}/{id}/</c>, also noch nicht in einen Jahresordner sortiert.</summary>
        public static bool IsUnsorted(StoredFile file)
        {
            var segments = file.Key.Split('/');
            return !file.IsTrash && segments.Length == 3 && segments[0] != UserFolder;
        }

        /// <summary>Der Schlüssel einer unsortierten Datei im Ordner ihres Upload-Jahres.</summary>
        public static string SortedKey(StoredFile file)
        {
            var segments = file.Key.Split('/');
            return Key(segments[0], segments[1], file.LastModified.ToLocalTime().Year, segments[2]);
        }

        /// <summary>Der Ordner des Objekts, in dem die Datei liegt (ohne Dateinamen).</summary>
        public static string Folder(string key) => key[..key.LastIndexOf('/')];
    }

    /// <summary>
    /// Zugriff auf die Dateiablage: ein S3-kompatibler Server hinter <c>S3_PROVIDER</c>, ohne
    /// Signatur (wie der übrige Dateizugriff). Verschieben ist Lesen, Schreiben, Löschen — das
    /// braucht kein serverseitiges Kopieren.
    /// </summary>
    public class EntityFileStore(HttpClient client, string baseUrl)
    {
        public string Url(string key) =>
            baseUrl.TrimEnd('/') + "/" + string.Join("/", key.Split('/').Select(Uri.EscapeDataString));

        /// <summary>Alle Objekte unter dem Präfix, seitenweise gelesen. Ein Fehler beendet die Liste.</summary>
        public async Task<List<StoredFile>> ListAsync(string prefix)
        {
            var files = new List<StoredFile>();
            string? marker = null;
            while (true)
            {
                var url = $"{baseUrl.TrimEnd('/')}?prefix={Uri.EscapeDataString(prefix)}"
                    + (marker == null ? "" : $"&marker={Uri.EscapeDataString(marker)}");

                XDocument document;
                try
                {
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) return files;
                    document = XDocument.Parse(await response.Content.ReadAsStringAsync());
                }
                catch (Exception ex) when (ex is XmlException or HttpRequestException)
                {
                    return files;
                }

                string? last = null;
                foreach (var contents in document.Descendants().Where(e => e.Name.LocalName == "Contents"))
                {
                    string? Value(string name) =>
                        contents.Elements().FirstOrDefault(e => e.Name.LocalName == name)?.Value;

                    var key = Value("Key");
                    if (key == null) continue;
                    last = key;
                    _ = DateTimeOffset.TryParse(Value("LastModified"), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal, out var modified);
                    _ = long.TryParse(Value("Size"), CultureInfo.InvariantCulture, out var size);
                    files.Add(new StoredFile(key, modified, size));
                }

                var truncated = document.Descendants().FirstOrDefault(e => e.Name.LocalName == "IsTruncated")?.Value;
                if (!string.Equals(truncated, "true", StringComparison.OrdinalIgnoreCase) || last == null)
                    return files;
                marker = document.Descendants().FirstOrDefault(e => e.Name.LocalName == "NextMarker")?.Value ?? last;
            }
        }

        /// <summary>
        /// Die Datei eines Objekts mit diesem Namen, egal in welchem Jahresordner (oder flach);
        /// bei mehreren die zuletzt geschriebene. Der Papierkorb zählt nicht.
        /// </summary>
        public async Task<StoredFile?> FindAsync(string entity, string id, string fileName) =>
            (await ListAsync($"{entity}/{id}/"))
                .Where(f => !f.IsTrash && f.FileName == fileName)
                .OrderByDescending(f => f.LastModified)
                .FirstOrDefault();

        /// <summary>Verschiebt ein Objekt; erst wenn das Ziel geschrieben ist, wird die Quelle gelöscht.</summary>
        public async Task<bool> MoveAsync(string from, string to)
        {
            var read = await client.GetAsync(Url(from));
            if (!read.IsSuccessStatusCode) return false;

            var content = new ByteArrayContent(await read.Content.ReadAsByteArrayAsync());
            content.Headers.ContentType = read.Content.Headers.ContentType is MediaTypeHeaderValue type ? type : null;
            var write = await client.PutAsync(Url(to), content);
            if (!write.IsSuccessStatusCode) return false;

            var delete = await client.DeleteAsync(Url(from));
            return delete.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string key) => (await client.DeleteAsync(Url(key))).IsSuccessStatusCode;
    }
}
