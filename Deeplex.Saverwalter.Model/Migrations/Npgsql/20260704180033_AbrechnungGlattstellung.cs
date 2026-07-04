using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deeplex.Saverwalter.Model.Migrations.Npgsql
{
    /// <summary>
    /// Stellt bestehende BK-Abrechnungssätze auf die Glattstellungs-Buchung um.
    ///
    /// Alt (3 Zeilen): Soll Vorauszahlung → NkBuchungskonto,
    ///                 Haben Rechnungsbetrag → BkAbrechnungsKonto,
    ///                 Saldo-Leg auf dem ZahlungsKonto (Soll = Nachzahlung, Haben = Guthaben).
    /// Neu (2 Zeilen): nur der Saldo, glattgestellt auf dem BkAbrechnungsKonto:
    ///                 Nachzahlung: Soll Saldo → BkAbrechnungsKonto / Haben Saldo → NkBuchungskonto,
    ///                 Guthaben:    Soll Saldo → NkBuchungskonto / Haben Saldo → BkAbrechnungsKonto.
    /// Damit endet das NkBuchungskonto pro Jahr auf 0 und das BkAbrechnungsKonto
    /// trägt genau den offenen Saldo (Nachforderung/Guthaben).
    ///
    /// Nur Sätze mit einer Zeile auf dem Vertrags-Zahlungskonto (= Altformat) werden
    /// angefasst; bereits glattgestellte Sätze bleiben unberührt. Auf diesen Sätzen
    /// existieren noch keine Ausgleichszahlungen/OPOS, daher ist das Löschen sicher.
    /// </summary>
    public partial class AbrechnungGlattstellung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TEMP TABLE _abr_glattstellung AS
SELECT s.buchungssatz_id AS satz,
       v.bk_abrechnungs_konto_id AS bkabr,
       v.nk_buchungskonto_id AS nk,
       CASE WHEN zk.soll_haben = 0 THEN zk.betrag ELSE -zk.betrag END AS saldo
FROM abrechnungsresultate r
JOIN buchungssaetze s ON s.buchungssatz_id = r.buchungssatz_id
JOIN vertraege v ON v.vertrag_id = r.vertrag_id
JOIN buchungszeilen zk ON zk.buchungssatz_id = s.buchungssatz_id
                       AND zk.buchungskonto_id = v.zahlungs_konto_id;

DELETE FROM buchungszeilen
WHERE buchungssatz_id IN (SELECT satz FROM _abr_glattstellung);

INSERT INTO buchungszeilen
    (buchungszeile_id, buchungssatz_id, buchungskonto_id, soll_haben, betrag, created_at, last_modified)
-- Nachzahlung (saldo > 0): Soll BkAbrechnungsKonto / Haben NkBuchungskonto
SELECT gen_random_uuid(), satz, bkabr, 0, saldo, now(), now()
FROM _abr_glattstellung WHERE saldo > 0
UNION ALL
SELECT gen_random_uuid(), satz, nk, 1, saldo, now(), now()
FROM _abr_glattstellung WHERE saldo > 0
-- Guthaben (saldo < 0): Soll NkBuchungskonto / Haben BkAbrechnungsKonto
UNION ALL
SELECT gen_random_uuid(), satz, nk, 0, -saldo, now(), now()
FROM _abr_glattstellung WHERE saldo < 0
UNION ALL
SELECT gen_random_uuid(), satz, bkabr, 1, -saldo, now(), now()
FROM _abr_glattstellung WHERE saldo < 0;

DROP TABLE _abr_glattstellung;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nicht umkehrbar: Rechnungsbetrag und Vorauszahlung lassen sich aus der
            // Glattstellung allein nicht rekonstruieren (nur ihr Saldo ist erhalten).
            throw new System.NotSupportedException(
                "Die Umstellung auf die Glattstellungs-Buchung kann nicht automatisch " +
                "rückgängig gemacht werden.");
        }
    }
}
