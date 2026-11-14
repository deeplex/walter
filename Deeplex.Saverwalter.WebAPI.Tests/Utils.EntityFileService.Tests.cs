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

using System.Text;
using System.Web;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Deeplex.Saverwalter.WebAPI.Tests
{
    /// <summary>Ein S3-Server im Speicher: Liste (seitenweise), Lesen, Schreiben, Löschen.</summary>
    public sealed class FakeS3 : HttpMessageHandler
    {
        public const string Base = "http://s3.test/bucket";
        public Dictionary<string, (byte[] Data, DateTimeOffset Modified)> Objects { get; } = [];
        public int PageSize { get; set; } = 1000;
        public DateTimeOffset Now { get; set; } = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

        public void Add(string key, string content, DateTimeOffset modified) =>
            Objects[key] = (Encoding.UTF8.GetBytes(content), modified);

        public string Text(string key) => Encoding.UTF8.GetString(Objects[key].Data);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = Uri.UnescapeDataString(request.RequestUri!.AbsolutePath).TrimStart('/');
            var key = path.StartsWith("bucket/") ? path["bucket/".Length..] : "";

            if (request.Method == HttpMethod.Get && key == "")
            {
                var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
                var prefix = query["prefix"] ?? "";
                var marker = query["marker"];
                var matching = Objects.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
                    .OrderBy(k => k, StringComparer.Ordinal)
                    .Where(k => marker == null || string.CompareOrdinal(k, marker) > 0)
                    .ToList();
                var page = matching.Take(PageSize).ToList();
                var xml = new StringBuilder("<ListBucketResult xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">");
                foreach (var k in page)
                    xml.Append($"<Contents><Key>{k}</Key><LastModified>{Objects[k].Modified:yyyy-MM-ddTHH:mm:ss.fffZ}</LastModified><Size>{Objects[k].Data.Length}</Size></Contents>");
                xml.Append($"<IsTruncated>{(matching.Count > page.Count ? "true" : "false")}</IsTruncated></ListBucketResult>");
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(xml.ToString()) };
            }
            if (request.Method == HttpMethod.Get)
            {
                return Objects.TryGetValue(key, out var found)
                    ? new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(found.Data) }
                    : new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }
            if (request.Method == HttpMethod.Put)
            {
                Objects[key] = (await request.Content!.ReadAsByteArrayAsync(cancellationToken), Now);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            if (request.Method == HttpMethod.Delete)
            {
                Objects.Remove(key);
                return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.MethodNotAllowed);
        }
    }

    public class EntityFileServiceTests
    {
        private static readonly DateTimeOffset Juni2022 = new(2022, 6, 15, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset Juni2024 = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

        private readonly FakeS3 _s3 = new();
        private EntityFileService Service => new(new HttpClient(_s3), FakeS3.Base);

        private static HttpRequest Request(string method, string body = "inhalt")
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bytes);
            context.Request.ContentLength = bytes.Length;
            return context.Request;
        }

        [Fact]
        public async Task UploadOhneJahrUndObjektjahrBleibtOhneJahr()
        {
            await Service.UploadAsync(Request("PUT"), "vertraege", "3", "a.pdf", null);

            _s3.Objects.Keys.Should().Equal("vertraege/3/a.pdf");
        }

        [Fact]
        public async Task UploadMitJahrLandetImGewaehltenJahr()
        {
            await Service.UploadAsync(Request("PUT"), "vertraege", "3", "a.pdf", 2023);

            _s3.Objects.Keys.Should().Equal("vertraege/3/2023/a.pdf");
        }

        [Fact]
        public async Task ErneuterUploadOhneJahrErsetztDieDateiInIhremJahr()
        {
            _s3.Add("vertraege/3/2022/a.pdf", "alt", Juni2022);

            await Service.UploadAsync(Request("PUT", "neu"), "vertraege", "3", "a.pdf", null);

            _s3.Objects.Keys.Should().Equal("vertraege/3/2022/a.pdf");
            _s3.Text("vertraege/3/2022/a.pdf").Should().Be("neu");
        }

        [Fact]
        public async Task UploadInAnderesJahrErsetztUndVerschiebtDieVorhandeneDatei()
        {
            _s3.Add("vertraege/3/2022/a.pdf", "alt", Juni2022);

            await Service.UploadAsync(Request("PUT", "neu"), "vertraege", "3", "a.pdf", 2024);

            _s3.Objects.Keys.Should().Equal("vertraege/3/2024/a.pdf");
            _s3.Text("vertraege/3/2024/a.pdf").Should().Be("neu");
        }

        [Fact]
        public async Task UploadLaesstFlacheDateienDesObjektsUnberuehrt()
        {
            _s3.Add("vertraege/3/alt.pdf", "alt", Juni2022);

            await Service.UploadAsync(Request("PUT"), "vertraege", "3", "neu.pdf", 2024);

            _s3.Objects.Keys.Should().BeEquivalentTo("vertraege/3/alt.pdf", "vertraege/3/2024/neu.pdf");
        }

        [Fact]
        public async Task UploadOhneJahrLegtDieDateiFlachAb()
        {
            await Service.UploadAsync(Request("PUT"), "vertraege", "3", "a.pdf", FileLayout.NoYear);

            _s3.Objects.Keys.Should().Equal("vertraege/3/a.pdf");
        }

        [Fact]
        public async Task ErneuterUploadEinerDateiOhneJahrBleibtOhneJahr()
        {
            _s3.Add("vertraege/3/a.pdf", "alt", Juni2022);

            await Service.UploadAsync(Request("PUT", "neu"), "vertraege", "3", "a.pdf", null, defaultYear: 2024);

            _s3.Objects.Keys.Should().Equal("vertraege/3/a.pdf");
            _s3.Text("vertraege/3/a.pdf").Should().Be("neu");
        }

        [Fact]
        public async Task ListeZeigtDasJahrAusDemOrdnerUndNullOhneJahr()
        {
            _s3.Add("vertraege/3/2024/sortiert.pdf", "x", Juni2022);
            _s3.Add("vertraege/3/flach.pdf", "x", Juni2022);
            _s3.Add("vertraege/3/2024/trash/geloescht.pdf", "", Juni2024);
            _s3.Add("vertraege/30/anderer.pdf", "x", Juni2022);

            var files = await Service.ListAsync("vertraege", "3", "/api/vertraege/3/files");

            files.Select(f => (f.FileName, f.Year, f.Key)).Should().BeEquivalentTo(new[]
            {
                ("sortiert.pdf", 2024, "/api/vertraege/3/files/sortiert.pdf"),
                ("flach.pdf", FileLayout.NoYear, "/api/vertraege/3/files/flach.pdf"),
            });
        }

        [Fact]
        public async Task FamilieListetDieDateienAllerMitgliederMitHerkunft()
        {
            _s3.Add("zaehler/1/2024/typenschild.jpg", "x", Juni2024);
            _s3.Add("zaehlerstaende/5/2024/stand.jpg", "x", Juni2024);
            _s3.Add("zaehlerstaende/6/flach.jpg", "x", Juni2022);
            _s3.Add("zaehlerstaende/7/2024/fremd.jpg", "x", Juni2024); // gehört nicht zur Familie
            var family = new FileFamily(
            [
                new("zaehler", "1", "Zähler"),
                new("zaehlerstaende", "5", "Zählerstand 31.12.2024"),
                new("zaehlerstaende", "6", "Zählerstand 31.12.2022"),
            ]);

            var files = await Service.ListFamilyAsync(family, "zaehlerstaende", "5");

            files.Select(f => (f.FileName, f.Key, f.Source, f.Year)).Should().BeEquivalentTo(new[]
            {
                ("typenschild.jpg", "/api/zaehler/1/files/typenschild.jpg", "Zähler", 2024),
                ("stand.jpg", "/api/zaehlerstaende/5/files/stand.jpg", (string?)null, 2024),
                ("flach.jpg", "/api/zaehlerstaende/6/files/flach.jpg", "Zählerstand 31.12.2022", FileLayout.NoYear),
            });
        }

        [Fact]
        public async Task FamilieOhneMitgliederZeigtNurDasObjektDerSeite()
        {
            _s3.Add("kontakte/9/2024/a.pdf", "x", Juni2024);

            var files = await Service.ListFamilyAsync(new FileFamily([]), "kontakte", "9");

            files.Select(f => (f.FileName, f.Source)).Should().Equal(("a.pdf", (string?)null));
        }

        [Fact]
        public async Task UploadOhneJahrNutztDasJahrDesObjekts()
        {
            await Service.UploadAsync(Request("PUT"), "zaehlerstaende", "5", "a.jpg", null, defaultYear: 2021);

            _s3.Objects.Keys.Should().Equal("zaehlerstaende/5/2021/a.jpg");
        }

        [Fact]
        public async Task GewaehltesJahrUndVorhandenesJahrGehenDemObjektjahrVor()
        {
            _s3.Add("zaehlerstaende/5/2022/b.jpg", "alt", Juni2022);

            await Service.UploadAsync(Request("PUT"), "zaehlerstaende", "5", "a.jpg", 2024, defaultYear: 2021);
            await Service.UploadAsync(Request("PUT", "neu"), "zaehlerstaende", "5", "b.jpg", null, defaultYear: 2021);

            _s3.Objects.Keys.Should().BeEquivalentTo("zaehlerstaende/5/2024/a.jpg", "zaehlerstaende/5/2022/b.jpg");
        }

        [Fact]
        public async Task LesenFindetDieDateiImJahresordnerUndImFlachenAlt()
        {
            _s3.Add("vertraege/3/2024/a.pdf", "im Jahr", Juni2024);
            _s3.Add("vertraege/3/b.pdf", "flach", Juni2022);

            var a = await Service.ReadOrDeleteAsync(Request("GET"), "vertraege", "3", "a.pdf");
            var b = await Service.ReadOrDeleteAsync(Request("GET"), "vertraege", "3", "b.pdf");

            Encoding.UTF8.GetString(a.Should().BeOfType<FileContentResult>().Subject.FileContents).Should().Be("im Jahr");
            Encoding.UTF8.GetString(b.Should().BeOfType<FileContentResult>().Subject.FileContents).Should().Be("flach");
        }

        [Fact]
        public async Task LoeschenLegtDenPapierkorbMarkerNebenDieDatei()
        {
            _s3.Add("vertraege/3/2024/a.pdf", "x", Juni2024);

            await Service.ReadOrDeleteAsync(Request("DELETE"), "vertraege", "3", "a.pdf");

            _s3.Objects.Keys.Should().Equal("vertraege/3/2024/trash/a.pdf");
        }

        [Fact]
        public async Task JahrWechselnVerschiebtDieDatei()
        {
            _s3.Add("vertraege/3/2022/a.pdf", "inhalt", Juni2022);

            var result = await Service.ChangeYearAsync("vertraege", "3", "a.pdf", 2024);

            result.Should().BeOfType<OkResult>();
            _s3.Objects.Keys.Should().Equal("vertraege/3/2024/a.pdf");
            _s3.Text("vertraege/3/2024/a.pdf").Should().Be("inhalt");
        }

        [Fact]
        public async Task JahrWechselnMitNullNimmtDerDateiDasJahr()
        {
            _s3.Add("vertraege/3/2022/a.pdf", "inhalt", Juni2022);

            var result = await Service.ChangeYearAsync("vertraege", "3", "a.pdf", FileLayout.NoYear);

            result.Should().BeOfType<OkResult>();
            _s3.Objects.Keys.Should().Equal("vertraege/3/a.pdf");
            _s3.Text("vertraege/3/a.pdf").Should().Be("inhalt");
        }

        [Fact]
        public async Task JahrWechselnEinerFlachenDateiSortiertSie()
        {
            _s3.Add("vertraege/3/a.pdf", "inhalt", Juni2022);

            await Service.ChangeYearAsync("vertraege", "3", "a.pdf", 2025);

            _s3.Objects.Keys.Should().Equal("vertraege/3/2025/a.pdf");
        }

        [Fact]
        public async Task JahrWechselnEinerUnbekanntenDateiIstNichtGefunden()
        {
            var result = await Service.ChangeYearAsync("vertraege", "3", "fehlt.pdf", 2024);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UmbenennenBleibtImJahresordner()
        {
            _s3.Add("vertraege/3/2022/alt.pdf", "inhalt", Juni2022);

            await Service.RenameAsync(Request("PUT", "inhalt"), "vertraege", "3", "alt.pdf", "neu.pdf");

            _s3.Objects.Keys.Should().Equal("vertraege/3/2022/neu.pdf");
        }

        [Fact]
        public async Task SortierenVerschiebtNurFlacheObjektDateienInIhrUploadJahr()
        {
            _s3.Add("vertraege/3/a.pdf", "a", Juni2022);
            _s3.Add("wohnungen/7/b.pdf", "b", Juni2024);
            _s3.Add("vertraege/3/2024/schon.pdf", "s", Juni2022);       // schon sortiert
            _s3.Add("vertraege/3/trash/weg.pdf", "", Juni2022);          // Papierkorb
            _s3.Add("user/abc/privat.pdf", "p", Juni2022);               // persönliche Datei
            var service = Service;

            var sorted = await service.SortAsync();
            var again = await service.SortAsync();

            sorted.Should().Be(2);
            again.Should().Be(0);
            _s3.Objects.Keys.Should().BeEquivalentTo(
                "vertraege/3/2022/a.pdf", "wohnungen/7/2024/b.pdf", "vertraege/3/2024/schon.pdf",
                "vertraege/3/trash/weg.pdf", "user/abc/privat.pdf");
            _s3.Text("vertraege/3/2022/a.pdf").Should().Be("a");
        }

        [Fact]
        public async Task SortierenLiestAlleSeitenDerListe()
        {
            _s3.PageSize = 2;
            for (var i = 0; i < 7; i++) _s3.Add($"vertraege/3/f{i}.pdf", $"{i}", Juni2022);

            var sorted = await Service.SortAsync();

            sorted.Should().Be(7);
            _s3.Objects.Keys.Should().OnlyContain(k => k.StartsWith("vertraege/3/2022/"));
        }

        [Fact]
        public async Task SortierenRaeumtEinDuplikatGleicherGroesseAufUndLaesstAbweichendesStehen()
        {
            _s3.Add("vertraege/3/gleich.pdf", "abc", Juni2022);
            _s3.Add("vertraege/3/2022/gleich.pdf", "abc", Juni2022);
            _s3.Add("vertraege/3/anders.pdf", "kurz", Juni2022);
            _s3.Add("vertraege/3/2022/anders.pdf", "viel laenger", Juni2022);

            await Service.SortAsync();

            _s3.Objects.Keys.Should().BeEquivalentTo(
                "vertraege/3/2022/gleich.pdf", "vertraege/3/anders.pdf", "vertraege/3/2022/anders.pdf");
        }
    }
}
