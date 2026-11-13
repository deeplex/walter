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
using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services.Abrechnung;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Der einzige Ort, an dem Buchungssätze gelöscht oder storniert werden (abgesehen von der
    /// Rückabwicklung des Abrechnungslaufs, die eine ganze Abrechnungsgruppe zurücknimmt und
    /// von hier aus angestoßen wird). Die Regel selbst steht in <see cref="BuchungssatzSchutzService"/>.
    ///
    /// <see cref="RueckgaengigAsync"/> wählt selbst, für eine zusammengehörige Menge von Sätzen
    /// (z. B. alle Sätze einer Transaktion):
    /// - <b>Gesperrt</b>, wenn ein Satz eine Stornobuchung oder bereits storniert ist.
    /// - <b>Storno</b>, wenn etwas daran hängt: ein Ausgleich mit einem anderen Satz oder eine
    ///   betroffene Abrechnung, die schon versendet ist. Grund ist Pflicht.
    /// - sonst <b>Löschen</b> — vollständig, als hätte es die Sätze nie gegeben.
    /// Sind die Sätze in Abrechnungen eingeflossen (oder selbst Abrechnungssätze), werden
    /// diese Abrechnungen als ganze Gruppe mit zurückgenommen: nicht versendet per
    /// Rückabwicklung, versendet per Storno. Das geschieht nur nach ausdrücklicher
    /// Bestätigung; das Jahr wird danach neu abgerechnet.
    ///
    /// <see cref="LoeschenAsync"/> und <see cref="StornierenAsync"/> sind die ausdrücklichen
    /// Wege der Expertenansicht: dieselbe Regel, aber ohne Abrechnungen mitzunehmen.
    /// </summary>
    public class RueckgaengigService(
        SaverwalterContext ctx,
        BuchungssatzSchutzService schutzService,
        StornoBuchungsService stornoService,
        AbrechnungslaufService abrechnungslauf,
        AbrechnungsgruppenService gruppenService)
    {
        public enum RueckgaengigArt { Loeschen, Storno, Gesperrt }

        public sealed class RueckgaengigRequest
        {
            public string? Grund { get; set; }
            /// <summary>Der Nutzer hat die Liste der mit zurückgenommenen Abrechnungen bestätigt.</summary>
            public bool AbrechnungenBestaetigt { get; set; }
        }

        public sealed record BetroffeneAbrechnung(
            int Jahr, List<int> WohnungIds, string Bezeichnung, bool Versendet);

        public sealed class RueckgaengigPlan
        {
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public RueckgaengigArt Art { get; init; }
            public string? Sperrgrund { get; init; }
            public List<BetroffeneAbrechnung> Abrechnungen { get; init; } = [];
        }

        public async Task<RueckgaengigPlan> PruefeAsync(IReadOnlyCollection<Buchungssatz> saetze)
        {
            if (saetze.Count == 0)
                return new RueckgaengigPlan { Art = RueckgaengigArt.Gesperrt, Sperrgrund = "Keine Buchungssätze vorhanden." };

            var hatAbhaengigkeit = false;
            var resultate = new List<Abrechnungsresultat>();
            foreach (var satz in saetze)
            {
                var schutz = await schutzService.PruefeAsync(satz);
                if (schutz.IstStorniert)
                    return Gesperrt($"Buchungssatz Nr. {satz.Buchungsnummer} ist bereits storniert.");
                if (schutz.IstStorno)
                    return Gesperrt($"Buchungssatz Nr. {satz.Buchungsnummer} ist eine Stornobuchung.");
                hatAbhaengigkeit |= schutz.HatOposVerknuepfung;
                resultate.AddRange(await schutzService.BetroffeneResultateAsync(satz));
            }

            var abrechnungen = await GruppierenAsync(resultate);
            return new RueckgaengigPlan
            {
                Art = hatAbhaengigkeit || abrechnungen.Any(a => a.Versendet)
                    ? RueckgaengigArt.Storno
                    : RueckgaengigArt.Loeschen,
                Abrechnungen = abrechnungen,
            };
        }

        /// <summary>
        /// Führt das Rückgängigmachen aus und gibt den ausgeführten Plan zurück. Wirft
        /// <see cref="InvalidOperationException"/>, wenn es gesperrt ist, betroffene
        /// Abrechnungen nicht bestätigt sind oder ein Storno-Grund fehlt.
        /// </summary>
        public async Task<RueckgaengigPlan> RueckgaengigAsync(
            IReadOnlyCollection<Buchungssatz> saetze, string? grund, bool abrechnungenBestaetigt)
        {
            var plan = await PruefeAsync(saetze);
            if (plan.Art == RueckgaengigArt.Gesperrt)
                throw new InvalidOperationException(plan.Sperrgrund);
            if (plan.Abrechnungen.Count > 0 && !abrechnungenBestaetigt)
                throw new InvalidOperationException(
                    "Betroffene Abrechnungen werden mit zurückgenommen — bitte bestätigen.");
            if (plan.Art == RueckgaengigArt.Storno && string.IsNullOrWhiteSpace(grund))
                throw new InvalidOperationException("Für ein Storno muss ein Grund angegeben werden.");

            var ids = saetze.Select(s => s.BuchungssatzId).ToList();
            await using var tx = await ctx.Database.BeginTransactionAsync();

            foreach (var abrechnung in plan.Abrechnungen)
            {
                if (abrechnung.Versendet)
                    await abrechnungslauf.StornoAsync(abrechnung.WohnungIds, abrechnung.Jahr, grund!.Trim());
                else
                    await abrechnungslauf.DeleteAsync(abrechnung.WohnungIds, abrechnung.Jahr);
            }

            // Abrechnungssätze sind mit ihrer Abrechnung erledigt; der Rest wird neu geladen.
            var verbleibend = await ctx.Buchungssaetze
                .Include(s => s.Buchungszeilen).ThenInclude(z => z.Buchungskonto)
                .Include(s => s.StornoNach)
                .Where(s => ids.Contains(s.BuchungssatzId) && s.StornoNach == null)
                .ToListAsync();

            if (plan.Art == RueckgaengigArt.Loeschen)
                await LoeschenOhneSpeichernAsync(verbleibend);
            else
                await StornierenOhneTransaktionAsync(verbleibend, grund!);

            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            return plan;
        }

        /// <summary>
        /// Löscht die Sätze vollständig. Wirft <see cref="InvalidOperationException"/> mit dem
        /// Grund, wenn ein Satz nicht löschbar ist; dann wird nichts gelöscht.
        /// </summary>
        public async Task LoeschenAsync(IReadOnlyCollection<Buchungssatz> saetze)
        {
            await LoeschenOhneSpeichernAsync(saetze);
            await ctx.SaveChangesAsync();
        }

        /// <summary>
        /// Storniert die Sätze (Gegenbuchungen, Grund als Notiz) und gibt die Storno-Sätze zurück.
        /// Wirft <see cref="InvalidOperationException"/> mit dem Grund, wenn ein Satz nicht
        /// stornierbar ist oder der Grund fehlt; dann wird nichts storniert.
        /// </summary>
        public async Task<List<Buchungssatz>> StornierenAsync(IReadOnlyCollection<Buchungssatz> saetze, string grund)
        {
            await using var tx = await ctx.Database.BeginTransactionAsync();
            var stornos = await StornierenOhneTransaktionAsync(saetze, grund);
            await tx.CommitAsync();
            return stornos;
        }

        private async Task LoeschenOhneSpeichernAsync(IReadOnlyCollection<Buchungssatz> saetze)
        {
            foreach (var satz in saetze)
            {
                var schutz = await schutzService.PruefeAsync(satz);
                if (!schutz.KannLoeschen)
                    throw new InvalidOperationException(Sperrmeldung(saetze, satz, schutz));
            }
            foreach (var satz in saetze)
                Entfernen(satz);
        }

        private async Task<List<Buchungssatz>> StornierenOhneTransaktionAsync(IReadOnlyCollection<Buchungssatz> saetze, string grund)
        {
            if (string.IsNullOrWhiteSpace(grund))
                throw new InvalidOperationException("Für ein Storno muss ein Grund angegeben werden.");

            foreach (var satz in saetze)
            {
                var schutz = await schutzService.PruefeAsync(satz);
                if (!schutz.KannStornieren)
                    throw new InvalidOperationException(Sperrmeldung(saetze, satz, schutz));
            }

            var stornos = new List<Buchungssatz>();
            foreach (var satz in saetze)
                stornos.Add(await stornoService.StornierenAsync(satz.BuchungssatzId, grund.Trim()));
            return stornos;
        }

        private static string Sperrmeldung(IReadOnlyCollection<Buchungssatz> saetze, Buchungssatz satz, BuchungssatzSchutz schutz) =>
            saetze.Count > 1
                ? $"Buchungssatz Nr. {satz.Buchungsnummer}: {schutz.Sperrgrund}"
                : schutz.Sperrgrund ?? "Nicht möglich.";

        /// <summary>Entfernt einen Satz samt der Ausgleiche zwischen seinen eigenen Zeilen.</summary>
        private void Entfernen(Buchungssatz satz)
        {
            var zeilen = satz.Buchungszeilen.ToHashSet();
            var interne = zeilen
                .SelectMany(z => z.AlsSollZeile)
                .Where(a => zeilen.Contains(a.HabenZeile))
                .ToList();
            ctx.OffenePostenAusgleiche.RemoveRange(interne);
            ctx.Buchungssaetze.Remove(satz);
        }

        private static RueckgaengigPlan Gesperrt(string grund) =>
            new() { Art = RueckgaengigArt.Gesperrt, Sperrgrund = grund };

        /// <summary>Betroffene Resultate je Abrechnungsgruppe und Jahr.</summary>
        private async Task<List<BetroffeneAbrechnung>> GruppierenAsync(List<Abrechnungsresultat> resultate)
        {
            if (resultate.Count == 0) return [];

            var gruppen = await gruppenService.GetGruppenAsync();
            var result = new List<BetroffeneAbrechnung>();
            foreach (var jahr in resultate.GroupBy(r => r.Buchungssatz.Buchungsjahr))
            {
                var wohnungIds = jahr.Select(r => r.Vertrag.Wohnung.WohnungId).ToHashSet();
                var passend = gruppen.Where(g => g.WohnungIds.Any(wohnungIds.Contains)).ToList();
                if (passend.Count == 0)
                    passend = [new AbrechnungsGruppe { WohnungIds = [.. wohnungIds], Bezeichnung = "Abrechnung" }];
                foreach (var gruppe in passend)
                {
                    var versendet = await ctx.Abrechnungsresultate.AnyAsync(r =>
                        gruppe.WohnungIds.Contains(r.Vertrag.Wohnung.WohnungId)
                        && r.Buchungssatz.Buchungsjahr == jahr.Key
                        && r.Buchungssatz.StornoNach == null
                        && r.Abgesendet);
                    result.Add(new BetroffeneAbrechnung(jahr.Key, gruppe.WohnungIds, gruppe.Bezeichnung, versendet));
                }
            }
            return result;
        }
    }
}
