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

using Deeplex.Saverwalter.PrintService;
using static Deeplex.Saverwalter.BetriebskostenabrechnungService.NkGruppenAbrechnungsService;

namespace Deeplex.Saverwalter.WebAPI.Services.Abrechnung
{
    public class AbrechnungslaufPrintService
    {
        private readonly AbrechnungslaufService _laufService;

        public AbrechnungslaufPrintService(AbrechnungslaufService laufService)
        {
            _laufService = laufService;
        }

        public sealed class PrintEntry
        {
            public required string Dateiname { get; init; }
            public required byte[] Inhalt { get; init; }
            public bool IstEntwurf { get; init; }
        }

        public Task<List<PrintEntry>> ErstellePdfsAsync(List<int> wohnungIds, int jahr, int? vertragId = null)
            => ErstelleAsync(wohnungIds, jahr, vertragId, "pdf");

        public Task<List<PrintEntry>> ErstelleDocxsAsync(List<int> wohnungIds, int jahr, int? vertragId = null)
            => ErstelleAsync(wohnungIds, jahr, vertragId, "docx");

        private async Task<List<PrintEntry>> ErstelleAsync(
            List<int> wohnungIds, int jahr, int? vertragId, string format)
        {
            var (preview, alleEinheiten) = await _laufService.PreviewWithEinheitenAsync(wohnungIds, jahr);

            var gebucht = preview.Resultate
                .Where(r => r.VertragId.HasValue)
                .ToDictionary(r => r.VertragId!.Value, r => r.GebuchterSaldo);

            var mieterParteien = alleEinheiten
                .SelectMany(e => e.Parteien)
                .Where(p => p.Vertrag != null)
                .Where(p => vertragId == null || p.Vertrag!.VertragId == vertragId)
                .GroupBy(p => (p.Vertrag!.VertragId, p.Nutzungsbeginn, p.Nutzungsende))
                .Select(g => g.First())
                .ToList();

            var entries = new List<PrintEntry>();
            foreach (var partei in mieterParteien)
            {
                NkDruckdaten druckdaten;
                try
                {
                    druckdaten = NkDruckdaten.Build(partei, alleEinheiten, jahr);
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                var vid = partei.Vertrag!.VertragId;
                var gebuchterSaldo = gebucht.GetValueOrDefault(vid);
                // druckdaten.Saldo = Vorauszahlung − Rechnungsbetrag; der gebuchte
                // Saldo hat die umgekehrte Vorzeichenkonvention (Rechnungsbetrag − VZ).
                var berechneterSaldo = druckdaten.Rechnungsbetrag - druckdaten.Vorauszahlung;
                var istEntwurf = !gebuchterSaldo.HasValue
                    || Math.Abs(gebuchterSaldo.Value - berechneterSaldo) > 0.005m;
                var entwurfGrund = !gebuchterSaldo.HasValue
                    ? "Noch nicht gebucht"
                    : "Buchungsstand stimmt nicht überein";

                using var stream = new MemoryStream();
                if (format == "docx")
                    druckdaten.SaveAsDocx(stream, istEntwurf: istEntwurf, entwurfGrund: entwurfGrund);
                else
                    druckdaten.SaveAsPdf(stream, istEntwurf: istEntwurf, entwurfGrund: entwurfGrund);
                stream.Position = 0;

                entries.Add(new PrintEntry
                {
                    Dateiname = BuildDateiname(partei, jahr, istEntwurf, format),
                    Inhalt = stream.ToArray(),
                    IstEntwurf = istEntwurf
                });
            }

            return entries;
        }

        private static string BuildDateiname(NkPartei partei, int jahr, bool istEntwurf, string format)
        {
            var wohnung = partei.Wohnung;
            var adresse = wohnung.Adresse?.Anschrift ?? "Unbekannt";
            var bez = wohnung.Bezeichnung;
            var suffix = istEntwurf ? "_ENTWURF" : "";
            return $"NK_{jahr}_{Sanitize(adresse)}_{Sanitize(bez)}{suffix}.{format}";
        }

        private static string Sanitize(string s) =>
            string.Concat(s.Select(c => char.IsLetterOrDigit(c) || c == '-' ? c : '_'));
    }
}
