// Copyright (c) 2026 Kai Lawrence
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
using System.Text;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.InitiateTestDbs.Templates
{
    /// <summary>
    /// Seeds realistic placeholder files into an S3-compatible bucket so the
    /// development setup has plausible attachments for vertrag, wohnung,
    /// erhaltungsaufwendung, kontakt and adresse entities.
    ///
    /// The Walter file API forwards plain HTTP requests to the configured
    /// S3 provider without signing, so the bucket must accept anonymous
    /// PUT/GET. The compose file configures MinIO accordingly for development.
    /// </summary>
    internal static class FileStorage
    {
        public static async Task SeedSampleFiles(
            SaverwalterContext ctx,
            string s3Provider,
            int seed)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

            if (!await WaitForS3(http, s3Provider))
            {
                Console.WriteLine($"S3 endpoint {s3Provider} not reachable - skipping file seeding.");
                return;
            }

            var random = new Random(seed);
            var uploaded = 0;

            uploaded += await SeedVertragsFiles(ctx, http, s3Provider, random);
            uploaded += await SeedWohnungsFiles(ctx, http, s3Provider, random);
            uploaded += await SeedErhaltungsaufwendungsFiles(ctx, http, s3Provider, random);
            uploaded += await SeedAdressFiles(ctx, http, s3Provider, random);
            uploaded += await SeedKontaktFiles(ctx, http, s3Provider, random);

            Console.WriteLine($"Seeded {uploaded} sample files into {s3Provider}.");
        }

        private static async Task<bool> WaitForS3(HttpClient http, string s3Provider)
        {
            if (!Uri.TryCreate(s3Provider, UriKind.Absolute, out var baseUri))
            {
                return false;
            }

            // Probe the bucket root with a HEAD or GET; treat any HTTP response
            // (including 403/404) as "reachable" because that proves there is a
            // server at the other end. Connection failures fall through to a
            // retry loop.
            var probe = new Uri(baseUri.GetLeftPart(UriPartial.Authority));
            for (var attempt = 0; attempt < 30; attempt++)
            {
                try
                {
                    using var response = await http.GetAsync(probe);
                    return true;
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(1000);
                }
                catch (TaskCanceledException)
                {
                    await Task.Delay(1000);
                }
            }

            return false;
        }

        private static async Task<int> SeedVertragsFiles(
            SaverwalterContext ctx,
            HttpClient http,
            string s3Provider,
            Random random)
        {
            var vertraege = await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Adresse)
                .OrderBy(v => v.VertragId)
                .Take(20)
                .ToListAsync();

            var count = 0;
            foreach (var vertrag in vertraege)
            {
                if (vertrag.Versionen.Count == 0) continue;

                var version = vertrag.Versionen.OrderBy(v => v.Beginn).First();
                var address = vertrag.Wohnung.Adresse?.Anschrift ?? "ohne Adresse";
                var vertragLines = new[]
                {
                    $"Vertrag-ID: {vertrag.VertragId}",
                    $"Wohnung: {vertrag.Wohnung.Bezeichnung}",
                    $"Anschrift: {address}",
                    $"Beginn: {version.Beginn:yyyy-MM-dd}",
                    $"Ende: {(vertrag.Ende?.ToString("yyyy-MM-dd") ?? "unbefristet")}",
                    $"Grundmiete: {version.Grundmiete.ToString("0.00", CultureInfo.InvariantCulture)} EUR",
                    $"Personenzahl: {version.Personenzahl}",
                };

                var ok = await PutPdf(http, s3Provider, $"vertraege/{vertrag.VertragId}/Mietvertrag.pdf",
                    "Mietvertrag (Demo)", vertragLines);
                if (ok) count++;

                if (random.Next(0, 4) == 0)
                {
                    var hausLines = new[]
                    {
                        "Demo-Hausordnung fuer das Mietverhaeltnis",
                        $"Vertrag {vertrag.VertragId}",
                        "Ruhezeiten 22:00 bis 07:00 und 13:00 bis 15:00",
                        "Treppenhausreinigung im wechselnden Turnus",
                        "Muelltrennung nach kommunaler Vorgabe",
                    };
                    var ok2 = await PutPdf(http, s3Provider, $"vertraege/{vertrag.VertragId}/Anlage_Hausordnung.pdf",
                        "Anlage zum Mietvertrag - Hausordnung", hausLines);
                    if (ok2) count++;
                }
            }

            return count;
        }

        private static async Task<int> SeedWohnungsFiles(
            SaverwalterContext ctx,
            HttpClient http,
            string s3Provider,
            Random random)
        {
            var wohnungen = await ctx.Wohnungen
                .Include(w => w.Adresse)
                .OrderBy(w => w.WohnungId)
                .Take(15)
                .ToListAsync();

            var count = 0;
            foreach (var wohnung in wohnungen)
            {
                var lines = new List<string>
                {
                    $"Wohnung-ID: {wohnung.WohnungId}",
                    $"Bezeichnung: {wohnung.Bezeichnung}",
                    $"Wohnflaeche: {wohnung.Wohnflaeche.ToString("0.00", CultureInfo.InvariantCulture)} m^2",
                    $"Adresse: {wohnung.Adresse?.Anschrift ?? "-"}",
                };
                if (await PutPdf(http, s3Provider, $"wohnungen/{wohnung.WohnungId}/Uebergabeprotokoll.pdf",
                    "Wohnungsuebergabe (Demo)", lines))
                {
                    count++;
                }

                if (random.Next(0, 3) == 0)
                {
                    var grundLines = new[]
                    {
                        "Skizze zum Grundriss (Demo)",
                        $"Bezeichnung: {wohnung.Bezeichnung}",
                        "Wohnen / Schlafen / Bad / Kueche / Flur",
                        "Ohne Massstab - dient nur Demonstrationszwecken",
                    };
                    if (await PutPdf(http, s3Provider, $"wohnungen/{wohnung.WohnungId}/Grundriss.pdf",
                        "Grundriss (Demo)", grundLines))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static async Task<int> SeedErhaltungsaufwendungsFiles(
            SaverwalterContext ctx,
            HttpClient http,
            string s3Provider,
            Random random)
        {
            var aufwendungen = await ctx.Erhaltungsaufwendungen
                .OrderBy(e => e.ErhaltungsaufwendungId)
                .Take(20)
                .ToListAsync();

            var count = 0;
            foreach (var aufwendung in aufwendungen)
            {
                var lines = new[]
                {
                    $"Rechnungsnummer: R-{aufwendung.ErhaltungsaufwendungId:D5}",
                    $"Bezeichnung: {aufwendung.Bezeichnung}",
                    $"Datum: {aufwendung.Datum:yyyy-MM-dd}",
                    $"Betrag: {aufwendung.Betrag.ToString("0.00", CultureInfo.InvariantCulture)} EUR",
                    "Position 1: Material",
                    "Position 2: Arbeitsleistung",
                    "Zahlungsziel 14 Tage netto",
                };
                if (await PutPdf(http, s3Provider, $"erhaltungsaufwendungen/{aufwendung.ErhaltungsaufwendungId}/Rechnung.pdf",
                    $"Handwerkerrechnung - {aufwendung.Bezeichnung}", lines))
                {
                    count++;
                }
            }

            return count;
        }

        private static async Task<int> SeedAdressFiles(
            SaverwalterContext ctx,
            HttpClient http,
            string s3Provider,
            Random random)
        {
            var adressen = await ctx.Adressen
                .OrderBy(a => a.AdresseId)
                .Take(8)
                .ToListAsync();

            var count = 0;
            foreach (var adresse in adressen)
            {
                var lines = new[]
                {
                    $"Hausordnung fuer {adresse.Anschrift}",
                    "1. Ruhezeiten beachten",
                    "2. Treppenhausreinigung im Turnus",
                    "3. Muelltrennung nach kommunaler Vorgabe",
                    "4. Fahrraeder im Kellerabteil abstellen",
                    "5. Kein Grillen mit offener Flamme auf dem Balkon",
                };
                if (await PutPdf(http, s3Provider, $"adressen/{adresse.AdresseId}/Hausordnung.pdf",
                    "Hausordnung (Demo)", lines))
                {
                    count++;
                }
            }

            return count;
        }

        private static async Task<int> SeedKontaktFiles(
            SaverwalterContext ctx,
            HttpClient http,
            string s3Provider,
            Random random)
        {
            var kontakte = await ctx.Kontakte
                .OrderBy(k => k.KontaktId)
                .Take(6)
                .ToListAsync();

            var count = 0;
            foreach (var kontakt in kontakte)
            {
                var displayName = string.IsNullOrEmpty(kontakt.Vorname)
                    ? kontakt.Name
                    : $"{kontakt.Vorname} {kontakt.Name}";
                var lines = new[]
                {
                    $"Vollmacht fuer {displayName}",
                    $"Kontakt-ID: {kontakt.KontaktId}",
                    "Diese Vollmacht ist ein Demo-Dokument und hat keine rechtliche Wirkung.",
                };
                if (await PutPdf(http, s3Provider, $"kontakte/{kontakt.KontaktId}/Vollmacht.pdf",
                    "Vollmacht (Demo)", lines))
                {
                    count++;
                }
            }

            return count;
        }

        private static async Task<bool> PutPdf(
            HttpClient http,
            string s3Provider,
            string objectKey,
            string title,
            IEnumerable<string> bodyLines)
        {
            var url = $"{s3Provider.TrimEnd('/')}/{objectKey}";
            var pdfBytes = MinimalPdf.Build(title, bodyLines);

            using var content = new ByteArrayContent(pdfBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            try
            {
                using var response = await http.PutAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"  ! upload failed for {objectKey}: HTTP {(int)response.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                Console.WriteLine($"  ! upload threw for {objectKey}: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Generates a hand-rolled, byte-accurate minimal PDF that PDF viewers
    /// (browser-embedded and standalone) accept. Avoids pulling in a font
    /// stack just to render a few demo lines.
    /// </summary>
    internal static class MinimalPdf
    {
        public static byte[] Build(string title, IEnumerable<string> bodyLines)
        {
            using var ms = new MemoryStream();
            var offsets = new List<long>();

            void Write(string s)
            {
                var b = Encoding.Latin1.GetBytes(s);
                ms.Write(b, 0, b.Length);
            }
            void Mark() => offsets.Add(ms.Position);

            Write("%PDF-1.4\n");
            Write("%âãÏÓ\n");

            Mark();
            Write("1 0 obj\n<</Type /Catalog /Pages 2 0 R>>\nendobj\n");

            Mark();
            Write("2 0 obj\n<</Type /Pages /Kids [3 0 R] /Count 1>>\nendobj\n");

            Mark();
            Write("3 0 obj\n<</Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources <</Font <</F1 5 0 R /F2 6 0 R>>>>>>\nendobj\n");

            var content = new StringBuilder();
            content.Append("BT /F2 18 Tf 72 740 Td (").Append(EscapePdf(title)).Append(") Tj ET\n");
            var y = 700;
            foreach (var line in bodyLines)
            {
                content.Append("BT /F1 12 Tf 72 ").Append(y).Append(" Td (").Append(EscapePdf(line)).Append(") Tj ET\n");
                y -= 18;
                if (y < 60) break;
            }
            var contentStr = content.ToString();
            var contentLen = Encoding.Latin1.GetByteCount(contentStr);

            Mark();
            Write($"4 0 obj\n<</Length {contentLen}>>\nstream\n");
            Write(contentStr);
            Write("endstream\nendobj\n");

            Mark();
            Write("5 0 obj\n<</Type /Font /Subtype /Type1 /BaseFont /Helvetica>>\nendobj\n");

            Mark();
            Write("6 0 obj\n<</Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold>>\nendobj\n");

            var xrefOffset = ms.Position;
            Write("xref\n0 7\n0000000000 65535 f \n");
            foreach (var off in offsets)
            {
                Write(off.ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
            }
            Write("trailer\n<</Size 7 /Root 1 0 R>>\nstartxref\n");
            Write(xrefOffset.ToString(CultureInfo.InvariantCulture));
            Write("\n%%EOF\n");

            return ms.ToArray();
        }

        private static string EscapePdf(string text)
        {
            // PDF text rendered with the standard Helvetica font is interpreted
            // using WinAnsi (a Windows-1252 superset of Latin1). We map any
            // character that is not representable to '?' and PDF-escape the
            // metacharacters '(', ')' and '\\'.
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                if (c == '\\') sb.Append("\\\\");
                else if (c == '(') sb.Append("\\(");
                else if (c == ')') sb.Append("\\)");
                else if (c >= 32 && c <= 126) sb.Append(c);
                else if (c >= 160 && c <= 255) sb.Append(c);
                else sb.Append('?');
            }
            return sb.ToString();
        }
    }
}
