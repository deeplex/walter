// Copyright (C) 2023-2026  Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

/**
 * `ignoriert`: für dieses Feld ist nichts nötig (z. B. keine Abrechnung); es ist weder
 * erledigt noch offen und wird blass dargestellt.
 */
export type MatrixStatus = 'erledigt' | 'teilweise' | 'offen' | 'ignoriert';

export interface MatrixZelle {
    status: MatrixStatus;
    /** Kurztext in der Zelle, z.B. ein Betrag */
    text?: string;
    /** Tooltip */
    titel?: string;
    /** Bezug der Zelle für den Klick, z.B. die Umlage-ID */
    ref?: string;
}

export interface MatrixSpalte {
    id: string;
    label: string;
}

export interface MatrixZeile {
    id: string;
    /** Voller Text der Zeile, für Suche und Tooltips. */
    label: string;
    /** Überschrift, unter der benachbarte Zeilen zusammengefasst werden, z. B. die Adresse. */
    gruppe?: string;
    /** Werte der Info-Spalten vor den Feldern, z. B. Wohnung und Mieter. */
    info?: string[];
    /** Zellen je Spalten-ID; fehlt die Spalte, gilt sie für diese Zeile nicht. */
    zellen: Record<string, MatrixZelle | undefined>;
}

export interface Matrix {
    /** Überschriften der Info-Spalten vor den Feldern; ohne sie steht nur `label`. */
    infoSpalten?: string[];
    spalten: MatrixSpalte[];
    zeilen: MatrixZeile[];
}

export const istOffen = (z: MatrixZelle | undefined) =>
    z !== undefined && (z.status === 'offen' || z.status === 'teilweise');

/** Anzahl der Zellen, die noch nicht erledigt sind. */
export function zaehleOffen(matrix: Matrix): number {
    let n = 0;
    for (const zeile of matrix.zeilen) {
        for (const spalte of matrix.spalten) {
            if (istOffen(zeile.zellen[spalte.id])) n++;
        }
    }
    return n;
}

/** Anzahl offener Zellen in den Zeilen einer Gruppe. */
export function zaehleOffeneInGruppe(
    matrix: Matrix,
    gruppe: string | undefined
): number {
    let n = 0;
    for (const zeile of matrix.zeilen) {
        if (zeile.gruppe !== gruppe) continue;
        for (const spalte of matrix.spalten) {
            if (istOffen(zeile.zellen[spalte.id])) n++;
        }
    }
    return n;
}

export interface MatrixAnsicht {
    spalten: MatrixSpalte[];
    zeilen: MatrixZeile[];
}

/**
 * Die sichtbaren Zeilen und Spalten bei „Nur Offene“ und Suchtext. Mehrere Begriffe
 * trennt man mit `;`; es müssen alle passen (wie in den Tabellen). Ein Begriff trifft
 * Zeilen (Adresse, Wohnung, Mieter …) oder Spalten (Kostenart, Monat …):
 * - Trifft er eine Zeile, bleibt sie mit allen Spalten sichtbar.
 * - Trifft er nur Spalten, bleiben nur diese Spalten und die Zeilen mit einem Feld darin.
 */
export function filtereMatrix(
    matrix: Matrix,
    nurOffene: boolean,
    suche: string
): MatrixAnsicht {
    const begriffe = suche
        .toLocaleLowerCase('de')
        .split(';')
        .map((b) => b.trim())
        .filter((b) => b.length > 0);
    const enthaelt = (text: string, b: string) =>
        text.toLocaleLowerCase('de').includes(b);

    let spalten = matrix.spalten;
    let zeilen = matrix.zeilen;

    if (begriffe.length > 0) {
        // Begriffe, die nur Spalten treffen, schränken die Spalten ein.
        const zeilenBegriffe: string[] = [];
        const spaltenBegriffe: string[] = [];
        for (const b of begriffe) {
            if (matrix.zeilen.some((z) => enthaelt(z.label, b))) {
                zeilenBegriffe.push(b);
            } else if (matrix.spalten.some((s) => enthaelt(s.label, b))) {
                spaltenBegriffe.push(b);
            } else {
                return { spalten, zeilen: [] };
            }
        }

        if (spaltenBegriffe.length > 0) {
            spalten = matrix.spalten.filter((s) =>
                spaltenBegriffe.some((b) => enthaelt(s.label, b))
            );
            zeilen = zeilen.filter((z) =>
                spalten.some((s) => z.zellen[s.id] !== undefined)
            );
        }
        zeilen = zeilen.filter((z) =>
            zeilenBegriffe.every((b) => enthaelt(z.label, b))
        );
    }

    if (nurOffene) {
        zeilen = zeilen.filter((z) =>
            spalten.some((s) => istOffen(z.zellen[s.id]))
        );
    }

    return { spalten, zeilen };
}

/** Wichtigkeit eines Zustands, wenn mehrere Umlagen in einem Feld zusammenfallen. */
const rang: Record<MatrixStatus, number> = {
    ignoriert: 0,
    erledigt: 1,
    teilweise: 2,
    offen: 3
};

const nachGruppeUndLabel = (a: MatrixZeile, b: MatrixZeile) =>
    (a.gruppe ?? '').localeCompare(b.gruppe ?? '', 'de') ||
    a.label.localeCompare(b.label, 'de');

interface UmlageFuerMatrix {
    id?: number;
    typ?: { id?: string | number; text?: string };
    ende?: string;
    selectedWohnungen?: { id?: string | number; text?: string }[];
    wohnungenKurz?: { id: number; adresse: string; bezeichnung: string }[];
    betriebskostenrechnungsJahre?: number[];
}

/**
 * Matrix Wohnung × Umlagetyp für ein Jahr. Die Spalten sind die Kostenarten
 * (Typen), nicht die einzelnen Umlagen, damit die Matrix auch bei vielen
 * Umlagen schmal bleibt. Eine Zelle verweist per `ref` auf ihre Umlage; eine
 * Rechnung färbt alle Wohnungen dieser Umlage zugleich. Gelten für eine
 * Wohnung mehrere Umlagen desselben Typs, zeigt die Zelle die erste offene.
 */
export function baueRechnungsMatrix(
    umlagen: UmlageFuerMatrix[],
    jahr: number,
    /**
     * Wohnungen, für die im Jahr eine Abrechnung nötig ist, mit dem Stand „alle Abrechnungen
     * gesendet“. Fehlt die Angabe, gelten alle als nötig und offen.
     */
    benoetigteWohnungen?: ReadonlyMap<number, boolean>
): Matrix {
    const aktiv = umlagen.filter(
        (u) =>
            u.id !== undefined &&
            (u.selectedWohnungen ?? []).length > 0 &&
            (!u.ende || new Date(u.ende).getFullYear() >= jahr)
    );

    const typId = (u: UmlageFuerMatrix) => String(u.typ?.id ?? u.typ?.text);

    const spalten = new Map<string, MatrixSpalte>();
    for (const u of aktiv) {
        spalten.set(typId(u), { id: typId(u), label: u.typ?.text || '—' });
    }

    const zeilen = new Map<string, MatrixZeile>();
    for (const u of aktiv) {
        const erledigt = (u.betriebskostenrechnungsJahre ?? []).includes(jahr);
        for (const w of u.selectedWohnungen ?? []) {
            const key = String(w.id ?? w.text);
            let zeile = zeilen.get(key);
            if (!zeile) {
                const kurz = (u.wohnungenKurz ?? []).find(
                    (k) => String(k.id) === key
                );
                zeile = kurz
                    ? {
                          id: key,
                          label: `${kurz.adresse} – ${kurz.bezeichnung}`,
                          gruppe: kurz.adresse,
                          info: [kurz.bezeichnung],
                          zellen: {}
                      }
                    : {
                          id: key,
                          label: w.text ?? '—',
                          info: [w.text ?? '—'],
                          zellen: {}
                      };
                zeilen.set(key, zeile);
            }
            const nichtNoetig =
                benoetigteWohnungen !== undefined &&
                !benoetigteWohnungen.has(Number(w.id));
            // Ist die Abrechnung gesendet, lässt sich nichts mehr eintragen.
            const abgeschlossen =
                benoetigteWohnungen?.get(Number(w.id)) === true;
            const status: MatrixStatus = nichtNoetig
                ? 'ignoriert'
                : erledigt || abgeschlossen
                  ? 'erledigt'
                  : 'offen';
            // Gelten mehrere Umlagen desselben Typs, zeigt das Feld die dringendste.
            const bisher = zeile.zellen[typId(u)];
            if (bisher && rang[bisher.status] >= rang[status]) continue;
            zeile.zellen[typId(u)] = {
                status,
                ref: String(u.id),
                titel: nichtNoetig
                    ? 'Keine Abrechnung nötig'
                    : abgeschlossen && !erledigt
                      ? 'Abrechnung gesendet, keine Rechnung mehr nötig'
                      : `${u.typ?.text ?? 'Umlage'} ${jahr}`
            };
        }
    }

    return {
        infoSpalten: ['Wohnung'],
        spalten: [...spalten.values()].sort((a, b) =>
            a.label.localeCompare(b.label, 'de')
        ),
        zeilen: [...zeilen.values()].sort(nachGruppeUndLabel)
    };
}

export interface MietMatrixMonatDaten {
    monat: string;
    soll: number;
    gezahlt: number;
    offen: number;
}

export interface MietMatrixZeileDaten {
    vertragId: number;
    vertragBezeichnung: string;
    adresse: string;
    wohnung: string;
    mieter: string;
    zahlungsempfaengerId?: number;
    monate: MietMatrixMonatDaten[];
}

const MONATE_KURZ = [
    'Jan',
    'Feb',
    'Mär',
    'Apr',
    'Mai',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Okt',
    'Nov',
    'Dez'
];

/**
 * Matrix Vertrag × Monat. Eine Zelle ist erledigt, wenn nichts mehr offen ist,
 * teilweise bei einer Teilzahlung. Monate, die noch nicht fällig sind, fehlen in
 * den Daten und bleiben ohne Zelle.
 */
function monatsZellen(
    monate: MietMatrixMonatDaten[]
): Record<string, MatrixZelle> {
    return Object.fromEntries(
        monate.map((m) => {
            const status: MatrixStatus =
                m.offen <= 0.005
                    ? 'erledigt'
                    : m.gezahlt > 0.005
                      ? 'teilweise'
                      : 'offen';
            return [
                String(Number(m.monat.slice(5, 7))),
                {
                    status,
                    ref: m.monat,
                    titel: `Soll ${m.soll.toFixed(2)} €, gezahlt ${m.gezahlt.toFixed(2)} €, offen ${m.offen.toFixed(2)} €`
                } satisfies MatrixZelle
            ];
        })
    );
}

const monatsSpalten = (): MatrixSpalte[] =>
    MONATE_KURZ.map((label, i) => ({ id: String(i + 1), label }));

export interface GarageMatrixZeileDaten {
    garageVertragId: number;
    bezeichnung: string;
    adresse: string;
    garage: string;
    mieter: string;
    zahlungsempfaengerId?: number;
    monate: MietMatrixMonatDaten[];
}

/** Matrix Garagenvertrag × Monat, gruppiert nach Adresse. */
export function baueGarageMatrix(daten: GarageMatrixZeileDaten[]): Matrix {
    return {
        infoSpalten: ['Garage', 'Mieter'],
        spalten: monatsSpalten(),
        zeilen: daten
            .map((z) => ({
                id: String(z.garageVertragId),
                label: `${z.adresse} – ${z.bezeichnung}`,
                gruppe: z.adresse,
                info: [z.garage, z.mieter],
                zellen: monatsZellen(z.monate)
            }))
            .sort(
                (a, b) =>
                    nachGruppeUndLabel(a, b) ||
                    (a.info[1] ?? '').localeCompare(b.info[1] ?? '', 'de')
            )
    };
}

export interface ZaehlerMatrixZeileDaten {
    id: number;
    kennnummer: string;
    typText: string;
    adresse: string;
    wohnung: string;
    /** Die Wohnung des Zählers; fehlt bei einem allgemeinen Zähler der Adresse. */
    wohnungId?: number | null;
    lastStand?: number;
    lastEinheit?: string;
    lastDatum?: string;
    staende: { datum: string; stand: number; einheit: string }[];
}

/** Der letzte bekannte Stand mit Einheit und Datum, z. B. „1.234 kWh (01.05.2025)“. */
export function letzterStandText(z: {
    lastStand?: number;
    lastEinheit?: string;
    lastDatum?: string;
}): string {
    if (z.lastStand === undefined || z.lastStand === null) return '–';
    const stand = z.lastStand.toLocaleString('de-DE', {
        maximumFractionDigits: 3
    });
    const wert = `${stand} ${z.lastEinheit ?? ''}`.trim();
    return z.lastDatum
        ? `${wert} (${new Date(z.lastDatum + 'T00:00:00').toLocaleDateString(
              'de-DE',
              {
                  day: '2-digit',
                  month: '2-digit',
                  year: 'numeric'
              }
          )})`
        : wert;
}

/**
 * Zähler × Jahr. Das ganze Jahr ist eine Spalte: ein Zähler ist erledigt, sobald
 * im Jahr ein Stand eingetragen ist. Weitere Stände (Auszug, Wechsel) lassen sich
 * im Dialog des Feldes ergänzen.
 */
export function baueZaehlerMatrix(
    daten: ZaehlerMatrixZeileDaten[],
    jahr: number,
    /**
     * Wohnungen, für die im Jahr eine Abrechnung nötig ist, mit dem Stand „alle Abrechnungen
     * gesendet“. Fehlt die Angabe, gelten alle als nötig und offen.
     */
    benoetigteWohnungen?: ReadonlyMap<number, boolean>
): Matrix {
    // Ein Zähler einer Wohnung ohne nötige Abrechnung braucht keinen Stand.
    const nichtNoetig = (z: ZaehlerMatrixZeileDaten) =>
        benoetigteWohnungen !== undefined &&
        z.wohnungId !== undefined &&
        z.wohnungId !== null &&
        !benoetigteWohnungen.has(z.wohnungId);
    // Ist die Abrechnung gesendet, lässt sich kein Stand mehr eintragen.
    const abgeschlossen = (z: ZaehlerMatrixZeileDaten) =>
        z.wohnungId !== undefined &&
        z.wohnungId !== null &&
        benoetigteWohnungen?.get(z.wohnungId) === true;

    return {
        infoSpalten: ['Wohnung', 'Zähler', 'Typ', 'Letzter Stand'],
        spalten: [{ id: 'stand', label: `Stand ${jahr}` }],
        zeilen: daten
            .map((z) => ({
                id: String(z.id),
                label: `${z.adresse} ${z.wohnung} ${z.kennnummer} ${z.typText}`,
                gruppe: z.adresse,
                info: [
                    z.wohnung || 'Allgemein',
                    z.kennnummer,
                    z.typText,
                    letzterStandText(z)
                ],
                zellen: {
                    stand: {
                        status: nichtNoetig(z)
                            ? ('ignoriert' as MatrixStatus)
                            : z.staende.length > 0 || abgeschlossen(z)
                              ? ('erledigt' as MatrixStatus)
                              : ('offen' as MatrixStatus),
                        ref: String(z.id),
                        text:
                            z.staende.length > 1
                                ? String(z.staende.length)
                                : undefined,
                        titel: nichtNoetig(z)
                            ? 'Keine Abrechnung nötig'
                            : z.staende.length > 0
                              ? `${z.staende.length} Stand/Stände im Jahr`
                              : abgeschlossen(z)
                                ? 'Abrechnung gesendet, kein Stand mehr nötig'
                                : 'Noch kein Stand im Jahr'
                    }
                }
            }))
            .sort(
                (a, b) =>
                    nachGruppeUndLabel(a, b) ||
                    (a.info[1] ?? '').localeCompare(b.info[1] ?? '', 'de')
            )
    };
}

/** Dialoginhalt zu einem Zählerfeld: die Stände des Jahres. */
export function zaehlerDetail(
    zeile: ZaehlerMatrixZeileDaten,
    zaehlerHref: string | undefined
): MatrixDetail {
    return {
        wertLabel: 'Stand',
        zusammenfassung: [
            { label: 'Stände im Jahr', wert: String(zeile.staende.length) },
            ...(zeile.lastStand !== undefined
                ? [
                      {
                          label: 'Letzter bekannter Stand',
                          wert: `${zeile.lastStand} ${zeile.lastEinheit ?? ''}`.trim()
                      }
                  ]
                : [])
        ],
        eintraege: zeile.staende.map((st) => ({
            datum: st.datum,
            text: 'Zählerstand',
            wert: `${st.stand} ${st.einheit}`.trim()
        })),
        link: zaehlerHref
            ? { href: zaehlerHref, label: 'Zum Zähler' }
            : undefined
    };
}

export function baueMietMatrix(daten: MietMatrixZeileDaten[]): Matrix {
    return {
        infoSpalten: ['Wohnung', 'Mieter'],
        spalten: monatsSpalten(),
        zeilen: daten
            .map((z) => ({
                id: String(z.vertragId),
                label: z.vertragBezeichnung,
                gruppe: z.adresse,
                info: [z.wohnung, z.mieter],
                zellen: monatsZellen(z.monate)
            }))
            .sort(
                (a, b) =>
                    nachGruppeUndLabel(a, b) ||
                    (a.info[1] ?? '').localeCompare(b.info[1] ?? '', 'de')
            )
    };
}

/** Anzahl der verschiedenen Bezüge (z.B. Umlagen) mit mindestens einer offenen Zelle. */
export function zaehleOffeneBezuege(matrix: Matrix): number {
    const refs = new Set<string>();
    for (const zeile of matrix.zeilen) {
        for (const spalte of matrix.spalten) {
            const z = zeile.zellen[spalte.id];
            if (istOffen(z) && z?.ref) refs.add(z.ref);
        }
    }
    return refs.size;
}

export interface MatrixDetailEintrag {
    datum?: string;
    text: string;
    /** Betrag in Euro; alternativ `wert` als fertig formatierter Text. */
    betrag?: number;
    wert?: string;
    href?: string;
}

/** Inhalt des Dialogs zu einem Feld: was schon eingetragen ist. */
export interface MatrixDetail {
    /** Überschrift der Wertspalte, Standard „Betrag“. */
    wertLabel?: string;
    zusammenfassung: { label: string; wert: string }[];
    eintraege: MatrixDetailEintrag[];
    link?: { href: string; label: string };
}

export const leeresDetail = (): MatrixDetail => ({
    zusammenfassung: [],
    eintraege: []
});

interface MietkontoZahlungDaten {
    datum: string;
    betrag: number;
    zahler?: string;
    art: string;
    garage?: string;
}

export interface MietkontoDaten {
    monate: {
        jahr: number;
        monat: number;
        zahlungen: MietkontoZahlungDaten[];
    }[];
}

const mietZahlungText = (z: MietkontoZahlungDaten) => {
    const art =
        z.art === 'Garage'
            ? `Garage ${z.garage ?? ''}`.trim()
            : z.art === 'NkVorauszahlung'
              ? 'NK-Vorauszahlung'
              : 'Kaltmiete';
    return z.zahler ? `${art} von ${z.zahler}` : art;
};

const eur = (b: number) =>
    b.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });

/** Dialoginhalt zu einem Mietfeld aus dem Mietkonto des Vertrags. */
export function mietDetail(
    monat: MietMatrixMonatDaten,
    konto: MietkontoDaten | undefined,
    mietkontoHref: string | undefined
): MatrixDetail {
    const jahr = Number(monat.monat.slice(0, 4));
    const nr = Number(monat.monat.slice(5, 7));
    const kontoMonat = konto?.monate.find(
        (m) => m.jahr === jahr && m.monat === nr
    );

    return {
        zusammenfassung: [
            { label: 'Soll', wert: eur(monat.soll) },
            { label: 'Gezahlt', wert: eur(monat.gezahlt) },
            { label: 'Offen', wert: eur(Math.max(monat.offen, 0)) }
        ],
        eintraege: (kontoMonat?.zahlungen ?? []).map((z) => ({
            datum: z.datum,
            text: mietZahlungText(z),
            betrag: z.betrag
        })),
        link: mietkontoHref
            ? { href: mietkontoHref, label: 'Zum Mietkonto' }
            : undefined
    };
}

export interface RechnungDaten {
    id: string;
    datum: string;
    betrag: number;
    betreffendesJahr: number;
    isBezahlt: boolean;
    umlage?: { id?: string | number };
}

/** Dialoginhalt zu einem Rechnungsfeld: die erfassten Rechnungen der Umlage im Jahr. */
export function rechnungsDetail(
    rechnungen: RechnungDaten[],
    umlageId: string,
    jahr: number,
    umlageHref: string | undefined
): MatrixDetail {
    const passende = rechnungen
        .filter(
            (r) =>
                String(r.umlage?.id) === umlageId && r.betreffendesJahr === jahr
        )
        .sort((a, b) => a.datum.localeCompare(b.datum));

    return {
        zusammenfassung: [
            {
                label: 'Rechnungen',
                wert: String(passende.length)
            },
            {
                label: 'Summe',
                wert: eur(passende.reduce((a, r) => a + r.betrag, 0))
            }
        ],
        eintraege: passende.map((r) => ({
            datum: r.datum,
            text: r.isBezahlt ? 'Rechnung (bezahlt)' : 'Rechnung (offen)',
            betrag: r.betrag,
            href: `/buchungssaetze/${r.id}`
        })),
        link: umlageHref ? { href: umlageHref, label: 'Zur Umlage' } : undefined
    };
}

export interface AbrechnungsMatrixZeileDaten {
    vertragId: number;
    vertragBezeichnung: string;
    adresse: string;
    wohnung: string;
    mieter: string;
    jahre: {
        jahr: number;
        gesendet: boolean;
        fristablauf: string;
        istAbgelaufen: boolean;
    }[];
}

/**
 * Matrix Vertrag × Abrechnungsjahr für die letzten Jahre bis `bisJahr`. Ein Feld ist
 * erledigt, wenn die Abrechnung des Jahres gesendet ist. Verträge, die in diesen
 * Jahren nichts abzurechnen haben, fehlen.
 */
export function baueAbrechnungsMatrix(
    daten: AbrechnungsMatrixZeileDaten[],
    bisJahr: number,
    anzahl = 5
): Matrix {
    const von = bisJahr - anzahl + 1;
    const spalten: MatrixSpalte[] = Array.from({ length: anzahl }, (_, i) => ({
        id: String(von + i),
        label: String(von + i)
    }));

    const zeilen = daten
        .map((z) => ({
            id: String(z.vertragId),
            label: z.vertragBezeichnung,
            gruppe: z.adresse,
            info: [z.wohnung, z.mieter],
            zellen: Object.fromEntries(
                z.jahre
                    .filter((j) => j.jahr >= von && j.jahr <= bisJahr)
                    .map((j) => {
                        const frist = new Date(
                            j.fristablauf + 'T00:00:00'
                        ).toLocaleDateString('de-DE', {
                            day: '2-digit',
                            month: '2-digit',
                            year: 'numeric'
                        });
                        return [
                            String(j.jahr),
                            {
                                status: j.gesendet
                                    ? ('erledigt' as MatrixStatus)
                                    : ('offen' as MatrixStatus),
                                ref: String(j.jahr),
                                titel: `${
                                    j.gesendet
                                        ? `Abrechnung ${j.jahr} gesendet`
                                        : `Abrechnung ${j.jahr} nicht gesendet, Fristablauf ${frist}${j.istAbgelaufen ? ' (abgelaufen)' : ''}`
                                }. Ein Klick öffnet den Abrechnungslauf.`
                            } satisfies MatrixZelle
                        ];
                    })
            )
        }))
        .filter((z) => Object.keys(z.zellen).length > 0)
        .sort(
            (a, b) =>
                nachGruppeUndLabel(a, b) ||
                (a.info[1] ?? '').localeCompare(b.info[1] ?? '', 'de')
        );

    return { infoSpalten: ['Wohnung', 'Mieter'], spalten, zeilen };
}
