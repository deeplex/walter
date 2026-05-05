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

using Deeplex.Saverwalter.BetriebskostenabrechnungService;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.InitiateTestDbs.Templates
{
    /// <summary>
    /// Erstellt Buchungssätze für die Jahres-BK-Abrechnung nach dem neuen Buchungsmodell.
    /// Verträge werden in zusammenhängende Komponenten gruppiert (transitiv über gemeinsame
    /// Abrechnungseinheiten). Pro Komponente wird erst vollständig validiert, dann gebucht.
    /// </summary>
    internal static class BkAbrechnungNeu
    {
        private record VertragPlan(
            Vertrag Vertrag,
            List<Abrechnungseinheit> Einheiten,
            List<Note> Notes);

        public static async Task BucheJahresabrechnungAsync(SaverwalterContext ctx, int jahr)
        {
            var abrechnungsTag = new DateOnly(jahr, 12, 31);

            var vertraege = await ctx.Vertraege
                .AsSplitQuery()
                .Include(v => v.Versionen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.NkVerrechnungsKonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Betriebskostenrechnungen).ThenInclude(r => r.Buchungssatz)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Wohnungen)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Zaehler)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Typ)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.HKVO)
                .Include(v => v.Wohnung).ThenInclude(w => w.Umlagen).ThenInclude(u => u.Betriebskostenrechnungen)
                .Include(v => v.NkBuchungskonto).ThenInclude(k => k.Buchungszeilen).ThenInclude(z => z.Buchungssatz)
                .Include(v => v.BkAbrechnungsKonto)
                .Include(v => v.Mietminderungen)
                .Include(v => v.Mieter)
                .ToListAsync();

            var aktive = vertraege.Where(v => VertragAktivInJahr(v, jahr)).ToList();
            Console.WriteLine($"Buche BK-Jahresabrechnung {jahr}: {aktive.Count} aktive Verträge...");

            // Phase 1: Abrechnungseinheiten für alle Verträge berechnen
            var plaene = aktive.Select(v =>
            {
                var notes = new List<Note>();
                var zeitraum = new Zeitraum(jahr, v);
                var einheiten = Abrechnungseinheit.GetAbrechnungseinheiten(v, zeitraum, notes);
                return new VertragPlan(v, einheiten, notes);
            }).ToList();

            // Phase 2: Zusammenhängende Komponenten bilden
            // Zwei Verträge sind verbunden wenn sie eine Abrechnungseinheit teilen (gleicher WohnungId-Set).
            var komponenten = FindeKomponenten(plaene);
            Console.WriteLine($"  → {komponenten.Count} zusammenhängende Komponenten gefunden.");

            int gebuchte = 0, übersprungene = 0;

            foreach (var komponente in komponenten)
            {
                var fehler = komponente
                    .SelectMany(p => p.Notes.Where(n => n.Severity == Severity.Error)
                        .Select(n => (p.Vertrag, n)))
                    .ToList();

                if (fehler.Count > 0)
                {
                    var bezeichnung = komponente.First().Vertrag.Wohnung.Adresse?.Strasse ?? $"Vertrag {komponente.First().Vertrag.VertragId}";
                    Console.WriteLine($"  [ÜBERSPRUNGEN] Komponente '{bezeichnung}' ({komponente.Count} Verträge):");
                    foreach (var (vertrag, note) in fehler)
                        Console.WriteLine($"    Vertrag {vertrag.VertragId}: {note.Message}");
                    übersprungene += komponente.Count;
                    continue;
                }

                foreach (var plan in komponente)
                    BucheVertrag(ctx, plan, jahr, abrechnungsTag);

                gebuchte += komponente.Count;
            }

            await ctx.SaveChangesAsync();
            Console.WriteLine($"BK-Jahresabrechnung {jahr}: {gebuchte} Verträge gebucht, {übersprungene} übersprungen.");
        }

        private static List<List<VertragPlan>> FindeKomponenten(List<VertragPlan> plaene)
        {
            // Union-Find über Index in plaene
            var parent = Enumerable.Range(0, plaene.Count).ToArray();

            int Find(int i)
            {
                while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; }
                return i;
            }

            void Union(int a, int b)
            {
                a = Find(a); b = Find(b);
                if (a != b) parent[a] = b;
            }

            // Für jeden eindeutigen WohnungId-Set: alle Verträge die diesen Set haben, zusammenführen
            var gruppeZuIndizes = new Dictionary<string, List<int>>();
            for (int i = 0; i < plaene.Count; i++)
            {
                foreach (var einheit in plaene[i].Einheiten)
                {
                    // Schlüssel = sortierte WohnungIds der Einheit (erste Umlage reicht, alle haben denselben Set)
                    var wohnungIds = einheit.Rechnungen.Keys
                        .FirstOrDefault()?.Wohnungen
                        .Select(w => w.WohnungId)
                        .OrderBy(id => id);
                    if (wohnungIds == null) continue;
                    var key = string.Join(",", wohnungIds);
                    if (!gruppeZuIndizes.TryGetValue(key, out var liste))
                        gruppeZuIndizes[key] = liste = [];
                    liste.Add(i);
                }
            }

            foreach (var liste in gruppeZuIndizes.Values)
                for (int j = 1; j < liste.Count; j++)
                    Union(liste[0], liste[j]);

            return plaene
                .Select((plan, i) => (plan, root: Find(i)))
                .GroupBy(x => x.root, x => x.plan)
                .Select(g => g.ToList())
                .ToList();
        }

        private static void BucheVertrag(SaverwalterContext ctx, VertragPlan plan, int jahr, DateOnly abrechnungsTag)
        {
            var vertrag = plan.Vertrag;

            // Step 1: Anteiliges Soll auf NkBuchungskonto je Betriebskostenrechnung
            foreach (var einheit in plan.Einheiten)
            {
                foreach (var umlage in einheit.Rechnungen.Keys)
                {
                    var anteil = einheit.GetAnteil(umlage);
                    if (anteil <= 0) continue;

                    foreach (var entry in einheit.Rechnungen[umlage])
                    {
                        if (entry.Rechnung is null || entry.Betrag <= 0) continue;

                        var betrag = Math.Round(entry.Betrag * anteil, 2);
                        if (betrag <= 0) continue;

                        AddZeile(entry.Rechnung.Buchungssatz, SollHaben.Soll, betrag, vertrag.NkBuchungskonto);
                    }
                }
            }

            // Step 2: NkBuchungskonto-Saldo gegen BkAbrechnungsKonto ausgleichen
            var nkSaldo = NkBuchungskontoSaldoImJahr(vertrag.NkBuchungskonto, jahr);
            if (nkSaldo == 0) return;

            var ausgleichSatz = new Buchungssatz(
                abrechnungsTag,
                $"BK-Abrechnung {jahr} Ausgleich – {vertrag.VertragId}");

            if (nkSaldo > 0)
            {
                AddZeile(ausgleichSatz, SollHaben.Haben, nkSaldo, vertrag.NkBuchungskonto);
                AddZeile(ausgleichSatz, SollHaben.Soll, nkSaldo, vertrag.BkAbrechnungsKonto);
            }
            else
            {
                var betrag = -nkSaldo;
                AddZeile(ausgleichSatz, SollHaben.Soll, betrag, vertrag.NkBuchungskonto);
                AddZeile(ausgleichSatz, SollHaben.Haben, betrag, vertrag.BkAbrechnungsKonto);
            }

            ctx.Buchungssaetze.Add(ausgleichSatz);
        }

        private static decimal NkBuchungskontoSaldoImJahr(Buchungskonto konto, int jahr)
        {
            var soll = konto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Soll && z.Buchungssatz.Buchungsdatum.Year == jahr)
                .Sum(z => z.Betrag);
            var haben = konto.Buchungszeilen
                .Where(z => z.SollHaben == SollHaben.Haben && z.Buchungssatz.Buchungsdatum.Year == jahr)
                .Sum(z => z.Betrag);
            return soll - haben;
        }

        private static bool VertragAktivInJahr(Vertrag vertrag, int jahr)
        {
            if (!vertrag.Versionen.Any()) return false;
            var beginn = vertrag.Versionen.Min(v => v.Beginn);
            if (beginn.Year > jahr) return false;
            if (vertrag.Ende.HasValue && vertrag.Ende.Value.Year < jahr) return false;
            return true;
        }

        private static void AddZeile(Buchungssatz satz, SollHaben sollHaben, decimal betrag, Buchungskonto konto)
        {
            var zeile = new Buchungszeile(sollHaben, betrag)
            {
                Buchungssatz = satz,
                Buchungskonto = konto
            };
            satz.Buchungszeilen.Add(zeile);
        }
    }
}
