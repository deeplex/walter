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

using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    /// <summary>
    /// Prüft die Annahmen, auf denen die Fachsicht beruht, gegen die Daten und listet, was
    /// sie verletzt — statt es stillschweigend zu überspielen:
    /// - <b>Mehrdeutige Konten</b>: ein Buchungskonto mit mehreren Besitzern
    ///   (<see cref="Kontorollen"/>). Seine Buchungen lassen sich nicht zuordnen.
    /// - <b>Zahlungen ohne Zuordnung</b>: eine nicht stornierte Zahlungszeile auf einem Konto,
    ///   dessen Rolle einen Posten verlangt (Miete, Garagenmiete, NK-Abrechnung, Rechnung),
    ///   ohne <see cref="Buchungszeile.ZugeordnetZu"/>. Beim Buchen entsteht das nicht; es
    ///   betrifft Bestand, dessen Zuordnung nicht festgehalten wurde. Stornierte Zahlungen
    ///   ohne Zuordnung stehen gesondert (nur zur Information).
    /// Liest nur.
    /// </summary>
    public static class Strukturpruefung
    {
        private static readonly KontoRolle[] PostenRollen =
        [
            KontoRolle.Kaltmiete, KontoRolle.Garagenmiete, KontoRolle.BkAbrechnung,
            KontoRolle.Verbindlichkeit, KontoRolle.Betriebskosten,
        ];

        public class MehrdeutigesKonto
        {
            public int KontoId { get; set; }
            public string Kontonummer { get; set; } = "";
            public string Bezeichnung { get; set; } = "";
            public List<string> Besitzer { get; set; } = [];
        }

        public class ZahlungOhneZuordnung
        {
            public Guid TransaktionId { get; set; }
            public DateOnly Datum { get; set; }
            public decimal Betrag { get; set; }
            public string Text { get; set; } = "";
            public EntityRef? Ref { get; set; }
            public bool Storniert { get; set; }
        }

        public class Ergebnis
        {
            public List<MehrdeutigesKonto> MehrdeutigeKonten { get; set; } = [];
            public List<ZahlungOhneZuordnung> ZahlungenOhneZuordnung { get; set; } = [];
            public bool Auffaellig => MehrdeutigeKonten.Count > 0 || ZahlungenOhneZuordnung.Any(z => !z.Storniert);
        }

        /// <param name="sichtbar">Konten, die der Nutzer sehen darf; <c>null</c>: alle.</param>
        public static async Task<Ergebnis> ErmittleAsync(SaverwalterContext ctx, IReadOnlySet<int>? sichtbar)
        {
            bool Sichtbar(int id) => sichtbar == null || sichtbar.Contains(id);
            var rollen = await Kontorollen.ErmittleAsync(ctx);

            var mehrdeutigIds = rollen.Mehrdeutig.Keys.Where(Sichtbar).ToList();
            var konten = await ctx.Buchungskonten
                .Where(k => mehrdeutigIds.Contains(k.BuchungskontoId))
                .ToDictionaryAsync(k => k.BuchungskontoId);
            var mehrdeutig = mehrdeutigIds
                .Select(id => new MehrdeutigesKonto
                {
                    KontoId = id,
                    Kontonummer = konten[id].Kontonummer,
                    Bezeichnung = konten[id].Bezeichnung,
                    Besitzer = rollen.Mehrdeutig[id].Select(b => $"{b.Besitzer} ({b.Rolle})").ToList(),
                })
                .OrderBy(k => k.Kontonummer)
                .ToList();

            var postenKonten = rollen.Eindeutig
                .Where(e => PostenRollen.Contains(e.Value.Rolle) && Sichtbar(e.Key))
                .Select(e => e.Key)
                .ToList();
            var geldKonten = rollen.Eindeutig
                .Where(e => e.Value.Rolle == KontoRolle.Geld)
                .Select(e => e.Key)
                .ToList();
            // Erst nur die Kandidaten (Zeilen ohne Zuordnung) als schmale Projektion, dann
            // gesondert, welche ihrer Buchungssätze eine Geldkonto-Zeile haben.
            var kandidaten = await ctx.Buchungszeilen
                .Where(z => postenKonten.Contains(EF.Property<int>(z, "BuchungskontoId"))
                    && z.ZugeordnetZu == null
                    && z.Buchungssatz.Transaktion != null
                    && z.Buchungssatz.StornoVon == null)
                .Select(z => new
                {
                    KontoId = EF.Property<int>(z, "BuchungskontoId"),
                    z.SollHaben,
                    z.Betrag,
                    SatzId = EF.Property<Guid>(z, "BuchungssatzId"),
                    TransaktionId = z.Buchungssatz.Transaktion!.TransaktionId,
                    Datum = z.Buchungssatz.Transaktion!.Zahlungsdatum,
                    Storniert = z.Buchungssatz.StornoNach != null,
                })
                .ToListAsync();

            var satzIds = kandidaten.Select(k => k.SatzId).Distinct().ToList();
            var mitGeld = satzIds.Count == 0
                ? []
                : (await ctx.Buchungszeilen
                    .Where(z => satzIds.Contains(EF.Property<Guid>(z, "BuchungssatzId"))
                        && geldKonten.Contains(EF.Property<int>(z, "BuchungskontoId")))
                    .Select(z => EF.Property<Guid>(z, "BuchungssatzId"))
                    .Distinct()
                    .ToListAsync()).ToHashSet();

            var ohneZuordnung = kandidaten
                .Where(k => mitGeld.Contains(k.SatzId))
                .Select(k =>
                {
                    var besitz = rollen.Eindeutig[k.KontoId];
                    var art = besitz.Rolle switch
                    {
                        KontoRolle.Kaltmiete => "Kaltmiete",
                        KontoRolle.Garagenmiete => "Garagenmiete",
                        KontoRolle.BkAbrechnung => k.SollHaben == SollHaben.Haben ? "Nachzahlung NK-Abrechnung" : "Erstattung NK-Abrechnung",
                        KontoRolle.Verbindlichkeit => "Rechnung",
                        _ => "Betriebskosten",
                    };
                    return new ZahlungOhneZuordnung
                    {
                        TransaktionId = k.TransaktionId,
                        Datum = k.Datum,
                        Betrag = k.Betrag,
                        Text = $"{art} – {besitz.Kontext}",
                        Ref = besitz.Ref,
                        Storniert = k.Storniert,
                    };
                })
                .OrderBy(z => z.Storniert)
                .ThenBy(z => z.Datum)
                .ToList();

            return new Ergebnis { MehrdeutigeKonten = mehrdeutig, ZahlungenOhneZuordnung = ohneZuordnung };
        }
    }
}
