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

import type { WalterZahlungsStatus } from './WalterZahlungsStatus';

// ── Antwort von GET /api/vertraege/{id}/mietkonto ───────────────────────────
// und GET /api/garage-vertraege/{id}/mietkonto (nur Garagenmiete, keine NK-VZ)

export type WalterMietkontoZahlungsArt =
    | 'Kaltmiete'
    | 'Garage'
    | 'NkVorauszahlung'
    | 'MietKorrektur';

export interface WalterMietkontoZahlung {
    buchungssatzId: string;
    transaktionId?: string | null;
    datum: string;
    betrag: number;
    zahler?: string | null;
    art: WalterMietkontoZahlungsArt;
    garage?: string | null;
}

export interface WalterMietkontoGarage {
    garageVertragId: number;
    kennung: string;
    faellig: number;
    bezahlt: number;
}

export interface WalterMietkontoMonat {
    jahr: number;
    monat: number;
    faellig: number;
    bezahlt: number;
    offen: number;
    kaltmiete: number;
    garagen: WalterMietkontoGarage[];
    zahlungen: WalterMietkontoZahlung[];
}

export interface WalterMietkontoNkJahr {
    jahr: number;
    faellig: number;
    bezahlt: number;
    offen: number;
    zahlungen: WalterMietkontoZahlung[];
}

export interface WalterMietkonto {
    monate: WalterMietkontoMonat[];
    nkVorauszahlungen: WalterMietkontoNkJahr[];
    nichtZugeordnet: WalterMietkontoZahlung[];
    rueckgaengigGemacht: WalterMietkontoZahlung[];
    offen: number;
}

// ── Zeilen für WalterZahlungsuebersicht ─────────────────────────────────────

/** Einzelne Zahlung unter einer Zeile der Zahlungsübersicht. */
export interface WalterZahlungsEintrag {
    id: string;
    datum: string;
    betrag: number;
    text: string;
    /** Ziel des Links „Details“: Transaktion, sonst Buchungssatz. */
    href: string;
    /** Für „Rückgängig machen“: die ganze Zahlung, sonst der einzelne Buchungssatz. */
    transaktionId?: string;
    buchungssatzId: string;
}

/** Teilbetrag einer Zeile, z. B. Kaltmiete und Garage eines Monats. */
export interface WalterZahlungsTeil {
    text: string;
    faellig: number;
    bezahlt: number;
}

/** Eine Zeile der Zahlungsübersicht: eine Periode bzw. Forderung. */
export interface WalterZahlungsZeile {
    id: string;
    periode: string;
    /** Link der Periode, z. B. zur Abrechnung. */
    href?: string;
    faellig: number;
    bezahlt: number;
    offen: number;
    aufschluesselung: WalterZahlungsTeil[];
    zahlungen: WalterZahlungsEintrag[];
    /**
     * Wofür sich hier eine Zahlung erfassen lässt: Monat (`yyyy-mm-01`) im Mietkonto,
     * Id der Abrechnung bei Nebenkostenabrechnungen.
     */
    erfassenFuer?: string;
    /** Status statt des aus Fällig/Bezahlt ermittelten, z. B. nicht versendet. */
    status?: WalterZahlungsStatus;
}

const MONAT = [
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

export function monatText(jahr: number, monat: number): string {
    return `${MONAT[monat - 1]} ${jahr}`;
}

/** Wofür die Zahlung war — aus Art und Bezug, nie aus Freitext. */
export function zahlungsArtText(z: WalterMietkontoZahlung): string {
    switch (z.art) {
        case 'Kaltmiete':
            return 'Kaltmiete';
        case 'Garage':
            return z.garage ? `Garage ${z.garage}` : 'Garage';
        case 'NkVorauszahlung':
            return 'NK-Vorauszahlung';
        case 'MietKorrektur':
            return 'Miet-Korrektur (zur Kaltmiete umgebucht)';
    }
}

export function zahlungsEintrag(
    z: WalterMietkontoZahlung
): WalterZahlungsEintrag {
    const art = zahlungsArtText(z);
    return {
        id: `${z.buchungssatzId}-${z.art}-${z.garage ?? ''}`,
        datum: z.datum,
        betrag: z.betrag,
        text: z.zahler ? `${art} – von ${z.zahler}` : art,
        href: z.transaktionId
            ? `/transaktionen/${z.transaktionId}`
            : `/buchungssaetze/${z.buchungssatzId}`,
        transaktionId: z.transaktionId ?? undefined,
        buchungssatzId: z.buchungssatzId
    };
}

function monatsZeile(m: WalterMietkontoMonat): WalterZahlungsZeile {
    const garagen = m.garagen.map((g) => ({
        text: `Garage ${g.kennung}`,
        faellig: g.faellig,
        bezahlt: g.bezahlt
    }));
    const kaltmieteBezahlt =
        m.bezahlt - garagen.reduce((s, g) => s + g.bezahlt, 0);
    // Garagenmietkonto: keine Kaltmiete.
    const teile = [
        ...(m.kaltmiete !== 0 || Math.abs(kaltmieteBezahlt) > 0.005
            ? [
                  {
                      text: 'Kaltmiete',
                      faellig: m.kaltmiete,
                      bezahlt: kaltmieteBezahlt
                  }
              ]
            : []),
        ...garagen
    ];
    // Aufgeschlüsselt nur, wenn der Monat aus mehreren Teilen besteht.
    const aufschluesselung = teile.length > 1 ? teile : [];
    return {
        id: `monat-${m.jahr}-${m.monat}`,
        periode: monatText(m.jahr, m.monat),
        faellig: m.faellig,
        bezahlt: m.bezahlt,
        offen: m.offen,
        aufschluesselung,
        zahlungen: m.zahlungen.map(zahlungsEintrag),
        erfassenFuer: `${m.jahr}-${String(m.monat).padStart(2, '0')}-01`
    };
}

function nkZeile(n: WalterMietkontoNkJahr): WalterZahlungsZeile {
    return {
        id: `nk-${n.jahr}`,
        periode: `NK-Vorauszahlungen ${n.jahr}`,
        faellig: n.faellig,
        bezahlt: n.bezahlt,
        offen: n.offen,
        aufschluesselung: [],
        zahlungen: n.zahlungen.map(zahlungsEintrag)
    };
}

/**
 * Zeilen des Mietkontos, neuestes Jahr zuerst: je Jahr die Monate (Kaltmiete und
 * Garagen), danach die NK-Vorauszahlungen des Jahres. Mit `jahr` nur dieses Jahr.
 */
export function mietkontoZeilen(
    konto: WalterMietkonto,
    jahr?: number
): WalterZahlungsZeile[] {
    const jahre = [
        ...new Set([
            ...konto.monate.map((m) => m.jahr),
            ...konto.nkVorauszahlungen.map((n) => n.jahr)
        ])
    ]
        .filter((j) => jahr === undefined || j === jahr)
        .sort((a, b) => b - a);

    return jahre.flatMap((j) => {
        const monate = konto.monate
            .filter((m) => m.jahr === j)
            .sort((a, b) => b.monat - a.monat)
            .map(monatsZeile);
        const nk = konto.nkVorauszahlungen
            .filter((n) => n.jahr === j)
            .map(nkZeile);
        return [...monate, ...nk];
    });
}

/** Wofür eine Zahlung im schlanken Erfassen gebucht wird. */
export type WalterZahlungsZiel =
    /** Miete eines Monats (`yyyy-mm-01`); der Server teilt auf. */
    | { art: 'miete'; vertragId: number; monat: string }
    /** Miete eines Garagenvertrags. */
    | {
          art: 'garage';
          garageVertragId: number;
          monat: string;
          kennung?: string;
      }
    /** Zahlung einer erfassten Betriebskostenrechnung. */
    | { art: 'bkRechnung'; buchungssatzId: string }
    /** Neue Betriebskostenrechnung, gleich bezahlt. */
    | { art: 'bkNeu'; umlageId: number; jahr: number }
    /** Nachzahlung oder Erstattung einer Nebenkostenabrechnung. */
    | { art: 'abrechnung'; abrechnungsresultatId: string; erstattung: boolean };
