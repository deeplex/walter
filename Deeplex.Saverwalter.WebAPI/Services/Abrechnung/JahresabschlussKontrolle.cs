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

using System.Text.Json.Serialization;

namespace Deeplex.Saverwalter.WebAPI.Services.Abrechnung
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PruefStatus
    {
        /// <summary>Gebucht und eine Neuberechnung ergäbe exakt dasselbe.</summary>
        Bestanden,
        /// <summary>Gebucht, aber eine Neuberechnung würde abweichen.</summary>
        NichtBestanden,
        /// <summary>Weder Abrechnung gebucht noch Verzicht — noch nicht gemacht.</summary>
        Fehlt,
        /// <summary>Verzicht dokumentiert und nichts gebucht — akzeptiert, aber kein echtes "Bestanden".</summary>
        Verzichtet
    }

    public class PruefPosition
    {
        public int? VertragId { get; init; }
        public string Bezeichnung { get; init; } = "";
        public string Gruppe { get; init; } = "";
        public PruefStatus Status { get; init; }
        /// <summary>Gebuchter Abrechnungs-Saldo (null wenn nicht gebucht).</summary>
        public decimal? GebuchterSaldo { get; init; }
        /// <summary>Saldo, den eine Neuberechnung jetzt ergäbe.</summary>
        public decimal? NeuerSaldo { get; init; }
        public string? Detail { get; init; }
    }

    public class JahresabschlussKontrolleResult
    {
        public int Jahr { get; init; }
        public List<PruefPosition> Positionen { get; init; } = [];
        public int Bestanden { get; set; }
        public int NichtBestanden { get; set; }
        public int Fehlt { get; set; }
        public int Verzichtet { get; set; }
        public int Gesamt => Positionen.Count;
    }

    /// <summary>
    /// Prüft je Vertrag, ob eine erneute Abrechnung dasselbe ergäbe wie das Gebuchte.
    /// Rein — arbeitet nur auf dem bereits berechneten <see cref="AbrechnungslaufGruppeResult"/>
    /// (der Saldo + Anteile jeweils geplant UND gebucht nebeneinander trägt).
    /// </summary>
    public static class JahresabschlussKontrolle
    {
        private const decimal Eps = 0.005m;

        public static IEnumerable<PruefPosition> Klassifiziere(
            AbrechnungslaufGruppeResult preview, string gruppe, HashSet<int> verzichteteVertragIds)
        {
            var alleAnteile = preview.Abrechnungseinheiten
                .SelectMany(e => e.NkZeilen)
                .SelectMany(z => z.Anteile)
                .ToList();

            foreach (var r in preview.Resultate.Where(r => r.VertragId.HasValue))
            {
                var vertragId = r.VertragId!.Value;
                var anteile = alleAnteile.Where(a => a.VertragId == vertragId).ToList();

                var hatResultat = r.GebuchterSaldo.HasValue;
                var hatGebuchteAnteile = anteile.Any(a => a.GebuchterBetrag.HasValue);
                var hatBuchungen = hatResultat || hatGebuchteAnteile;
                var verzicht = verzichteteVertragIds.Contains(vertragId);

                var bezeichnung = string.IsNullOrWhiteSpace(r.MieterBezeichnung)
                    ? r.WohnungBezeichnung
                    : $"{r.MieterBezeichnung} – {r.WohnungBezeichnung}";

                PruefStatus status;
                string? detail = null;

                if (!hatBuchungen)
                {
                    status = verzicht ? PruefStatus.Verzichtet : PruefStatus.Fehlt;
                }
                else
                {
                    // Nur prüfen, was tatsächlich gebucht ist — ob Resultat/Anteile überhaupt
                    // gebucht wurden, ist eine Frage offener Posten, nicht der Konsistenz.
                    var saldoOk = !hatResultat
                        || Math.Abs(r.Saldo - r.GebuchterSaldo!.Value) <= Eps;
                    var anteileOk = anteile.All(a =>
                        !a.GebuchterBetrag.HasValue
                        || (a.GeplanterBetrag.HasValue
                            && Math.Abs(a.GebuchterBetrag.Value - a.GeplanterBetrag.Value) <= Eps));

                    if (saldoOk && anteileOk)
                    {
                        status = PruefStatus.Bestanden;
                    }
                    else
                    {
                        status = PruefStatus.NichtBestanden;
                        detail = !saldoOk
                            ? $"Saldo weicht ab: gebucht {r.GebuchterSaldo:0.00} €, neu {r.Saldo:0.00} €."
                            : "NK-Anteile weichen von der Neuberechnung ab.";
                    }
                }

                yield return new PruefPosition
                {
                    VertragId = vertragId,
                    Bezeichnung = bezeichnung,
                    Gruppe = gruppe,
                    Status = status,
                    GebuchterSaldo = r.GebuchterSaldo,
                    NeuerSaldo = r.Saldo,
                    Detail = detail
                };
            }

            // Verwaiste Eigenanteile (Leerstand): gebucht, aber von der Neuberechnung nicht
            // (gleich) reproduziert → eigene „nicht bestanden"-Position je Gruppe. Fängt Reste
            // einer alten, unvollständig zurückgenommenen Abrechnung.
            var eigenAnteileOrphan = alleAnteile.Any(a =>
                !a.VertragId.HasValue
                && a.GebuchterBetrag.HasValue
                && (!a.GeplanterBetrag.HasValue
                    || Math.Abs(a.GebuchterBetrag.Value - a.GeplanterBetrag.Value) > Eps));

            if (eigenAnteileOrphan)
                yield return new PruefPosition
                {
                    VertragId = null,
                    Bezeichnung = "Eigenanteil (Leerstand)",
                    Gruppe = gruppe,
                    Status = PruefStatus.NichtBestanden,
                    Detail = "Gebuchte Eigenanteile weichen von der Neuberechnung ab (evtl. Reste einer alten Abrechnung)."
                };
        }

        public static void Aggregiere(JahresabschlussKontrolleResult result)
        {
            result.Bestanden = result.Positionen.Count(p => p.Status == PruefStatus.Bestanden);
            result.NichtBestanden = result.Positionen.Count(p => p.Status == PruefStatus.NichtBestanden);
            result.Fehlt = result.Positionen.Count(p => p.Status == PruefStatus.Fehlt);
            result.Verzichtet = result.Positionen.Count(p => p.Status == PruefStatus.Verzichtet);
        }
    }
}
