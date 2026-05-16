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

namespace Deeplex.Saverwalter.InitiateTestDbs.Templates
{
    /// <summary>
    /// Füllt die Bücher aus historischen Daten (Mieten, Betriebskostenrechnungen).
    /// Einmalig auf einer frischen Datenbank ausführen.
    /// </summary>
    internal static class BuchungssaetzeErstellen
    {
        public static async Task BucheHistorischAsync(SaverwalterContext ctx)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            Console.WriteLine("Buche Mietsollstellungen...");
            await BuecheMietsollAsync(ctx, today);

            Console.WriteLine("Buche Mietzahlungen (aus legacy Miete-Tabelle)...");
            await BuecheMietzahlungenAsync(ctx);

            Console.WriteLine("Buche BK-Rechnungseingänge...");
            await BuecheBkEingangAsync(ctx);

            Console.WriteLine("Buche BK-Zahlungen...");
            await BuecheBkZahlungAsync(ctx);

            Console.WriteLine("Speichere...");
            await ctx.SaveChangesAsync();
            Console.WriteLine("Fertig.");
        }

        /// <summary>
        /// Für jeden Monat in dem ein Vertrag aktiv war: Mietsoll buchen.
        ///   Soll:  Vertrag.MietBuchungskonto  (Mietforderung entsteht)
        ///   Haben: Wohnung.MietErtragskonto   (Ertrag wird realisiert)
        /// </summary>
        private static async Task BuecheMietsollAsync(SaverwalterContext ctx, DateOnly today)
        {
            var vertraege = await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.Mieter)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Besitzer)
                .ToListAsync();

            foreach (var vertrag in vertraege)
            {
                if (!vertrag.Versionen.Any()) continue;

                var fruehsteVersion = vertrag.Versionen.MinBy(v => v.Beginn)!;
                var start = new DateOnly(fruehsteVersion.Beginn.Year, fruehsteVersion.Beginn.Month, 1);
                var end = vertrag.Ende ?? today;

                for (var monat = start; monat <= end; monat = monat.AddMonths(1))
                {
                    var version = vertrag.Versionen
                        .Where(v => v.Beginn <= monat)
                        .MaxBy(v => v.Beginn);

                    if (version is null) continue;

                    var satz = new Buchungssatz(monat, $"Mietsoll {monat:MM/yyyy}");
                    var transaktion = new Transaktion
                    {
                        Zahlungsdatum = monat,
                        Betrag = version.Grundmiete,
                        Verwendungszweck = satz.Beschreibung,
                        Zahler = vertrag.Mieter.FirstOrDefault(),
                        Zahlungsempfaenger = vertrag.Wohnung.Besitzer,
                    };
                    satz.Transaktion = transaktion;

                    var soll = new Buchungszeile(SollHaben.Soll, version.Grundmiete);
                    soll.Buchungskonto = vertrag.MietBuchungskonto;
                    soll.Buchungssatz = satz;

                    var haben = new Buchungszeile(SollHaben.Haben, version.Grundmiete);
                    haben.Buchungskonto = vertrag.Wohnung.MietErtragskonto;
                    haben.Buchungssatz = satz;

                    satz.Buchungszeilen.Add(soll);
                    satz.Buchungszeilen.Add(haben);
                    ctx.Transaktionen.Add(transaktion);
                    ctx.Buchungssaetze.Add(satz);
                }
            }
        }

        /// <summary>
        /// Für jede Miete (legacy): Zahlung buchen, aufgeteilt in Kaltmiete und NK-Anteil.
        ///   Haben: Vertrag.MietBuchungskonto  (Forderung wird beglichen)
        ///   Haben: Vertrag.NkBuchungskonto    (NK-Vorauszahlung eingenommen, falls Betrag > Grundmiete)
        ///   Soll:  Vertrag.ZahlungsKonto      (Geld kommt auf Zahlungskonto an)
        /// </summary>
#pragma warning disable CS0618
        private static async Task BuecheMietzahlungenAsync(SaverwalterContext ctx)
        {
            var vertraege = await ctx.Vertraege
                .Include(v => v.Versionen)
                .Include(v => v.MietBuchungskonto)
                .Include(v => v.NkBuchungskonto)
                .Include(v => v.ZahlungsKonto)
                .Include(v => v.Mieten)
                .Include(v => v.Mieter)
                .Include(v => v.Wohnung)
                    .ThenInclude(w => w.Besitzer)
                .ToListAsync();
#pragma warning restore CS0618

            foreach (var vertrag in vertraege)
            {
                foreach (var miete in vertrag.Mieten)
                {
                    var betrag = (decimal)miete.Betrag;
                    if (betrag <= 0) continue;

                    var version = vertrag.Versionen
                        .Where(v => v.Beginn <= miete.BetreffenderMonat)
                        .MaxBy(v => v.Beginn);

                    var grundmiete = version?.Grundmiete ?? betrag;
                    var mieteAnteil = Math.Min(betrag, grundmiete);
                    var nkAnteil = Math.Max(0, betrag - grundmiete);

                    var satz = new Buchungssatz(miete.Zahlungsdatum, $"Mietzahlung {miete.BetreffenderMonat:MM/yyyy}");
                    var transaktion = new Transaktion
                    {
                        Zahlungsdatum = miete.Zahlungsdatum,
                        Betrag = betrag,
                        Verwendungszweck = satz.Beschreibung,
                        Zahler = vertrag.Mieter.FirstOrDefault(),
                        Zahlungsempfaenger = vertrag.Wohnung.Besitzer,
                    };
                    satz.Transaktion = transaktion;

                    var habenMiet = new Buchungszeile(SollHaben.Haben, mieteAnteil);
                    habenMiet.Buchungskonto = vertrag.MietBuchungskonto;
                    habenMiet.Buchungssatz = satz;
                    satz.Buchungszeilen.Add(habenMiet);

                    var sollZahlung = new Buchungszeile(SollHaben.Soll, mieteAnteil);
                    sollZahlung.Buchungskonto = vertrag.ZahlungsKonto;
                    sollZahlung.Buchungssatz = satz;
                    satz.Buchungszeilen.Add(sollZahlung);

                    if (nkAnteil > 0)
                    {
                        var habenNk = new Buchungszeile(SollHaben.Haben, nkAnteil);
                        habenNk.Buchungskonto = vertrag.NkBuchungskonto;
                        habenNk.Buchungssatz = satz;
                        satz.Buchungszeilen.Add(habenNk);

                        var sollNk = new Buchungszeile(SollHaben.Soll, nkAnteil);
                        sollNk.Buchungskonto = vertrag.ZahlungsKonto;
                        sollNk.Buchungssatz = satz;
                        satz.Buchungszeilen.Add(sollNk);
                    }

                    ctx.Transaktionen.Add(transaktion);
                    ctx.Buchungssaetze.Add(satz);
                }
            }
        }

        /// <summary>
        /// Für jede Betriebskostenrechnung: Kosteneingang buchen (unausgeglichen — Gegenkonto folgt mit BK-Abrechnung).
        ///   Haben: Umlage.NkVerrechnungsKonto  (Kosten-Pool wächst)
        /// </summary>
        private static async Task BuecheBkEingangAsync(SaverwalterContext ctx)
        {
            var umlagen = await ctx.Umlagen
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.Typ)
                .Include(u => u.Betriebskostenrechnungen)
                .ToListAsync();

            foreach (var umlage in umlagen)
            {
                foreach (var rechnung in umlage.Betriebskostenrechnungen)
                {
                    var satz = new Buchungssatz(
                        rechnung.Datum,
                        $"BK-Eingang {umlage.Typ.Bezeichnung} {rechnung.BetreffendesJahr}");
                    var transaktion = new Transaktion
                    {
                        Zahlungsdatum = rechnung.Datum,
                        Betrag = rechnung.Betrag,
                        Verwendungszweck = satz.Beschreibung,
                    };
                    satz.Transaktion = transaktion;

                    var haben = new Buchungszeile(SollHaben.Haben, rechnung.Betrag);
                    haben.Buchungskonto = umlage.NkVerrechnungsKonto;
                    haben.Buchungssatz = satz;
                    satz.Buchungszeilen.Add(haben);

                    rechnung.Buchungssatz = satz;
                    ctx.Transaktionen.Add(transaktion);
                    ctx.Buchungssaetze.Add(satz);
                }
            }
        }

        /// <summary>
        /// Für jede Betriebskostenrechnung: Zahlung buchen.
        ///   Soll:  Umlage.NkVerrechnungsKonto  (Kosten-Pool wird abgebaut)
        ///   Haben: Umlage.ZahlungsKonto         (Geld geht aus)
        /// </summary>
        private static async Task BuecheBkZahlungAsync(SaverwalterContext ctx)
        {
            var umlagen = await ctx.Umlagen
                .Include(u => u.NkVerrechnungsKonto)
                .Include(u => u.ZahlungsKonto)
                .Include(u => u.Typ)
                .Include(u => u.Betriebskostenrechnungen)
                .ToListAsync();

            foreach (var umlage in umlagen)
            {
                foreach (var rechnung in umlage.Betriebskostenrechnungen)
                {
                    var satz = new Buchungssatz(
                        rechnung.Datum,
                        $"BK-Zahlung {umlage.Typ.Bezeichnung} {rechnung.BetreffendesJahr}");
                    var transaktion = new Transaktion
                    {
                        Zahlungsdatum = rechnung.Datum,
                        Betrag = rechnung.Betrag,
                        Verwendungszweck = satz.Beschreibung,
                    };
                    satz.Transaktion = transaktion;

                    var soll = new Buchungszeile(SollHaben.Soll, rechnung.Betrag);
                    soll.Buchungskonto = umlage.NkVerrechnungsKonto;
                    soll.Buchungssatz = satz;
                    satz.Buchungszeilen.Add(soll);

                    var haben = new Buchungszeile(SollHaben.Haben, rechnung.Betrag);
                    haben.Buchungskonto = umlage.ZahlungsKonto;
                    haben.Buchungssatz = satz;
                    satz.Buchungszeilen.Add(haben);

                    ctx.Transaktionen.Add(transaktion);
                    ctx.Buchungssaetze.Add(satz);
                }
            }
        }
    }
}
