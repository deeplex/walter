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

namespace Deeplex.Saverwalter.WebAPI.Services.Buchungen
{
    /// <summary>
    /// Die Sollstellung eines Monats auf einem Miet- oder Garagenkonto — eine Regel für
    /// Buchen, Mietkonto und Startseite. Sie ist eine Soll-Zeile, deren Satz
    /// - keine Transaktion trägt (keine Zahlung),
    /// - weder Storno noch storniert ist und
    /// - nicht auch das NK-Konto des Vertrags bebucht (das wäre eine Miet-Korrektur, die ihr
    ///   Buchungsdatum trägt, nicht den Monat einer Miete).
    /// Der Monat ist der des Buchungsdatums (per Konstruktion der dritte Werktag).
    ///
    /// Die Zeilen brauchen geladen: Buchungssatz mit Transaktion, StornoVon, StornoNach und
    /// den Buchungszeilen samt Buchungskonto.
    /// </summary>
    public static class Sollstellung
    {
        /// <summary>Schlüssel der Datenbank-Sperre für das Anlegen von Sollstellungen.</summary>
        private const long SperrSchluessel = 0x57414C5445520001; // "WALTER" + 1

        private static readonly SemaphoreSlim ProzessSperre = new(1, 1);

        /// <summary>
        /// Führt aus, was fehlende Sollstellungen anlegt (prüfen, anlegen, speichern), ohne
        /// dass ein anderer Vorgang gleichzeitig dasselbe tut — sonst legen zwei Anfragen
        /// denselben Monat beide an. Auf PostgreSQL ist es eine Transaktions-Sperre in der
        /// Datenbank (<c>pg_advisory_xact_lock</c>): sie gilt über Prozesse und Instanzen
        /// hinweg und endet mit der Transaktion. Läuft schon eine Transaktion, gilt die Sperre
        /// bis zu deren Ende. Ohne PostgreSQL (Tests) eine prozessweite Sperre.
        /// </summary>
        public static async Task<T> GesperrtAsync<T>(SaverwalterContext ctx, Func<Task<T>> aktion)
        {
            if (!ctx.Database.IsNpgsql())
            {
                await ProzessSperre.WaitAsync();
                try { return await aktion(); }
                finally { ProzessSperre.Release(); }
            }

            await using var eigene = ctx.Database.CurrentTransaction == null
                ? await ctx.Database.BeginTransactionAsync()
                : null;
            await ctx.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({SperrSchluessel})");
            var ergebnis = await aktion();
            if (eigene != null) await eigene.CommitAsync();
            return ergebnis;
        }

        /// <param name="nkKontoId">NK-Konto des Vertrags; bei Garagenkonten <c>null</c>.</param>
        public static bool Ist(Buchungszeile z, int? nkKontoId = null) =>
            z.SollHaben == SollHaben.Soll
            && z.Buchungssatz.Transaktion == null
            && z.Buchungssatz.StornoVon == null
            && z.Buchungssatz.StornoNach == null
            && (nkKontoId is not int nk
                || !z.Buchungssatz.Buchungszeilen.Any(g => g.Buchungskonto.BuchungskontoId == nk));

        public static bool IstImMonat(Buchungszeile z, DateOnly monat, int? nkKontoId = null) =>
            Ist(z, nkKontoId)
            && z.Buchungssatz.Buchungsdatum.Year == monat.Year
            && z.Buchungssatz.Buchungsdatum.Month == monat.Month;

        /// <summary>
        /// Die Monate eines Jahres, für die eine Sollstellung fällig ist: ab Vertragsbeginn
        /// (bzw. Januar), bis zum Vertragsende und höchstens bis zum laufenden Monat. Künftige
        /// Jahre haben keine. Jeweils der Erste des Monats.
        /// </summary>
        public static IEnumerable<DateOnly> Monate(int jahr, DateOnly heute, DateOnly beginn, DateOnly? ende)
        {
            var jahresanfang = new DateOnly(jahr, 1, 1);
            var bis = heute.Year == jahr
                ? new DateOnly(heute.Year, heute.Month, 1)
                : heute.Year > jahr
                    ? new DateOnly(jahr, 12, 31)
                    : jahresanfang.AddMonths(-1);
            if (bis < jahresanfang) yield break;

            if (ende is DateOnly e && e < bis)
                bis = new DateOnly(e.Year, e.Month, 1);

            for (var monat = new DateOnly(jahr, beginn.Year < jahr ? 1 : beginn.Month, 1); monat <= bis; monat = monat.AddMonths(1))
                yield return monat;
        }

        /// <summary>Die Sollstellung der Kaltmiete eines Monats: Soll Mietkonto, Haben Mietertragskonto der Wohnung.</summary>
        public static Buchungssatz ErstelleMiete(Vertrag vertrag, DateOnly monat, decimal grundmiete) =>
            Erstelle($"Mietsoll {monat:MM/yyyy}", monat, grundmiete,
                vertrag.MietBuchungskonto, vertrag.Wohnung.MietErtragskonto);

        /// <summary>Die Sollstellung der Garagenmiete eines Monats: Soll Garagen-Mietkonto, Haben Ertragskonto der Garage.</summary>
        public static Buchungssatz ErstelleGarage(GarageVertrag gv, DateOnly monat, decimal garagenMiete) =>
            Erstelle($"Garagenmietsoll {monat:MM/yyyy} | {gv.Garage.Kennung}", monat, garagenMiete,
                gv.MietBuchungskonto, gv.Garage.Ertragskonto);

        private static Buchungssatz Erstelle(string beschreibung, DateOnly monat, decimal betrag, Buchungskonto soll, Buchungskonto haben)
        {
            var satz = new Buchungssatz(DateUtils.DritterWerktag(monat), beschreibung);
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Soll, betrag) { Buchungssatz = satz, Buchungskonto = soll });
            satz.Buchungszeilen.Add(new Buchungszeile(SollHaben.Haben, betrag) { Buchungssatz = satz, Buchungskonto = haben });
            return satz;
        }

        /// <summary>Die Soll-Zeile eines mit <see cref="ErstelleMiete"/> bzw. <see cref="ErstelleGarage"/> erzeugten Satzes.</summary>
        public static Buchungszeile SollZeile(Buchungssatz satz) =>
            satz.Buchungszeilen.First(z => z.SollHaben == SollHaben.Soll);

        /// <summary>Per Ausgleich an der Sollstellung bezahlter Betrag.</summary>
        public static decimal Bezahlt(Buchungszeile sollstellung) =>
            sollstellung.AlsSollZeile.Sum(a => a.HabenZeile.Betrag);
    }
}
