// Copyright (c) 2023-2026 Kai Lawrence
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

using System.Text;
using Deeplex.Saverwalter.WebAPI.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    /// <summary>
    /// Läuft nur, wenn <c>WALTER_TEST_S3</c> gesetzt ist: die Adresse eines vorhandenen Buckets, der
    /// unsignierte Anfragen annimmt, z. B. <c>http://s3:9000/saverwalter</c> (wie <c>S3_PROVIDER</c>).
    /// </summary>
    public sealed class S3FactAttribute : FactAttribute
    {
        public const string Variable = "WALTER_TEST_S3";

        public S3FactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
                Skip = $"Umgebungsvariable {Variable} nicht gesetzt (Adresse eines S3-Buckets).";
        }
    }

    /// <summary>
    /// Die Dateiablage gegen einen echten S3-Server statt des Fakes: Schreiben, Lesen, Listen
    /// (seitenweise), Löschen mit Papierkorb-Marker, Jahr wechseln, Sortieren. Jeder Test arbeitet
    /// unter einer eigenen Objekt-Id und räumt sie wieder auf.
    /// </summary>
    public class EntityFileServiceS3Tests : IAsyncLifetime
    {
        private readonly string _base = Environment.GetEnvironmentVariable(S3FactAttribute.Variable) ?? "";
        private readonly string _id = Guid.NewGuid().ToString("N");
        private readonly HttpClient _client = new();
        private EntityFileService Service => new(_client, _base);
        private EntityFileStore Store => new(_client, _base);

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            if (string.IsNullOrEmpty(_base)) return;
            foreach (var file in await Store.ListAsync($"vertraege/{_id}/"))
                await Store.DeleteAsync(file.Key);
            foreach (var file in await Store.ListAsync($"paging-{_id}/"))
                await Store.DeleteAsync(file.Key);
            _client.Dispose();
        }

        private static HttpRequest Request(string method, string body = "inhalt")
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bytes);
            context.Request.ContentLength = bytes.Length;
            return context.Request;
        }

        private async Task<string> LiesAsync(string fileName)
        {
            var result = await Service.ReadOrDeleteAsync(Request("GET", ""), "vertraege", _id, fileName);
            return Encoding.UTF8.GetString(result.Should().BeOfType<FileContentResult>().Subject.FileContents);
        }

        private Task<List<WalterFile>> ListeAsync() => Service.ListAsync("vertraege", _id, $"/api/vertraege/{_id}/files");

        [S3Fact]
        public async Task SchreibenLesenUndListenMitJahr()
        {
            await Service.UploadAsync(Request("PUT", "pdf-inhalt"), "vertraege", _id, "a.pdf", 2023);

            (await LiesAsync("a.pdf")).Should().Be("pdf-inhalt");
            var liste = await ListeAsync();
            var datei = liste.Should().ContainSingle().Subject;
            datei.FileName.Should().Be("a.pdf");
            datei.Year.Should().Be(2023);
            datei.Size.Should().Be(10);
            datei.LastModified.Should().BeGreaterThan(0);
        }

        [S3Fact]
        public async Task DateinameMitUmlautenUndLeerzeichen()
        {
            await Service.UploadAsync(Request("PUT", "x"), "vertraege", _id, "Müller Nachtrag (2).pdf", null);

            (await LiesAsync("Müller Nachtrag (2).pdf")).Should().Be("x");
            (await ListeAsync()).Should().ContainSingle().Which.FileName.Should().Be("Müller Nachtrag (2).pdf");
        }

        [S3Fact]
        public async Task ErneuterUploadErsetztDieDateiInIhremJahr()
        {
            await Service.UploadAsync(Request("PUT", "alt"), "vertraege", _id, "a.pdf", 2022);
            await Service.UploadAsync(Request("PUT", "neu"), "vertraege", _id, "a.pdf", null);

            (await LiesAsync("a.pdf")).Should().Be("neu");
            (await ListeAsync()).Should().ContainSingle().Which.Year.Should().Be(2022);
        }

        [S3Fact]
        public async Task JahrWechselnVerschiebtDieDatei()
        {
            await Service.UploadAsync(Request("PUT", "inhalt"), "vertraege", _id, "a.pdf", 2022);

            (await Service.ChangeYearAsync("vertraege", _id, "a.pdf", 2024)).Should().BeOfType<OkResult>();

            (await LiesAsync("a.pdf")).Should().Be("inhalt");
            (await ListeAsync()).Should().ContainSingle().Which.Year.Should().Be(2024);
            (await Store.ListAsync($"vertraege/{_id}/")).Select(f => f.Key)
                .Should().Equal($"vertraege/{_id}/2024/a.pdf");
        }

        [S3Fact]
        public async Task UmbenennenBleibtImJahresordner()
        {
            await Service.UploadAsync(Request("PUT", "inhalt"), "vertraege", _id, "alt.pdf", 2023);

            await Service.RenameAsync(Request("PUT", "inhalt"), "vertraege", _id, "alt.pdf", "neu.pdf");

            (await Store.ListAsync($"vertraege/{_id}/")).Select(f => f.Key)
                .Should().Equal($"vertraege/{_id}/2023/neu.pdf");
        }

        [S3Fact]
        public async Task LoeschenLegtDenPapierkorbMarkerAnUndBlendetDieDateiAus()
        {
            await Service.UploadAsync(Request("PUT", "inhalt"), "vertraege", _id, "a.pdf", 2023);

            await Service.ReadOrDeleteAsync(Request("DELETE", ""), "vertraege", _id, "a.pdf");

            (await ListeAsync()).Should().BeEmpty();
            (await Store.ListAsync($"vertraege/{_id}/")).Select(f => f.Key)
                .Should().Equal($"vertraege/{_id}/2023/trash/a.pdf");
        }

        [S3Fact]
        public async Task SortierenVerschiebtFlacheDateienInIhrUploadJahr()
        {
            await Service.UploadAsync(Request("PUT", "inhalt"), "vertraege", _id, "flach.pdf", null);
            var uploadJahr = (await Store.ListAsync($"vertraege/{_id}/")).Single().LastModified.UtcDateTime.Year;

            (await Service.SortAsync($"vertraege/{_id}/")).Should().Be(1);

            (await Store.ListAsync($"vertraege/{_id}/")).Select(f => f.Key)
                .Should().Equal($"vertraege/{_id}/{uploadJahr}/flach.pdf");
            (await LiesAsync("flach.pdf")).Should().Be("inhalt");
            (await Service.SortAsync($"vertraege/{_id}/")).Should().Be(0);
        }

        [S3Fact]
        public async Task ListeLiestAlleSeitenEinerGrossenListe()
        {
            const int anzahl = 1105;
            using var gate = new SemaphoreSlim(16);
            await Task.WhenAll(Enumerable.Range(0, anzahl).Select(async i =>
            {
                await gate.WaitAsync();
                try
                {
                    var response = await _client.PutAsync(
                        Store.Url($"paging-{_id}/f{i:D5}.txt"), new ByteArrayContent([1]));
                    response.EnsureSuccessStatusCode();
                }
                finally { gate.Release(); }
            }));

            var liste = await Store.ListAsync($"paging-{_id}/");

            liste.Should().HaveCount(anzahl);
            liste.Select(f => f.Key).Should().OnlyHaveUniqueItems();
        }
    }
}
