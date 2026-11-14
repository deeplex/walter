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
import { formatMonth } from '../services/format';
import type { WalterZuordnungEintrag } from './WalterTransaktion';
import {
    zuordnungAsParts,
    type WalterListItem,
    type WalterListItemTagColor
} from './WalterListItem';

// ── Antwort von GET /api/vertraege/{id}/mietkonto ───────────────────────────
// und GET /api/garage-vertraege/{id}/mietkonto (nur Garagenmiete, keine NK-VZ)

export type WalterMietkontoZahlungsArt =
    | 'Kaltmiete'
    | 'Garage'
    | 'NkVorauszahlung';

/** Transaktion, die (auch) die Miete eines Monats bezahlt hat. */
export interface WalterMietkontoTransaktion {
    transaktionId: string;
    datum: string;
    /** Betrag der ganzen Transaktion. */
    betrag: number;
    zahler?: string | null;
    /** Davon NK-Vorauszahlung des Vertrags (zählt für ihr Buchungsjahr). */
    nkVorauszahlung: number;
    /** Wofür die Transaktion gebucht wurde. */
    zuordnung: WalterZuordnungEintrag[];
}

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
    /** Per Ausgleich an den Sollstellungen des Monats hängende Zahlungszeilen. */
    zahlungen: WalterMietkontoZahlung[];
    /** Die Transaktionen hinter `zahlungen`. */
    transaktionen: WalterMietkontoTransaktion[];
    transaktionenBetrag: number;
    nkVorauszahlung: number;
}

export interface WalterMietkonto {
    monate: WalterMietkontoMonat[];
    nichtZugeordnet: WalterMietkontoZahlung[];
    rueckgaengigGemacht: WalterMietkontoZahlung[];
    offen: number;
}

// ── Zeilen für WalterZahlungsuebersicht ─────────────────────────────────────

/**
 * Einzelne Zahlung unter einer Zeile der Zahlungsübersicht. Der Link führt zur
 * Transaktion, sonst zum Buchungssatz; bei einer ganzen Transaktion sind die Teile
 * ihre Zuordnung.
 */
export interface WalterZahlungsEintrag extends WalterListItem {
    datum: string;
    betrag: number;
    href: string;
    /** Für „Rückgängig machen“: die ganze Zahlung, sonst der einzelne Buchungssatz. */
    transaktionId?: string;
    buchungssatzId?: string;
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
    breakdown: WalterZahlungsTeil[];
    zahlungen: WalterZahlungsEintrag[];
    /** Weitere Abschnitte im aufgeklappten Teil vor den Zahlungen, z. B. die Verteilung. */
    sections?: { title: string; items: WalterListItem[] }[];
    /**
     * Wofür sich hier eine Zahlung erfassen lässt: Monat (`yyyy-mm-01`) im Mietkonto,
     * Id der Abrechnung bei Nebenkostenabrechnungen, Id des Buchungssatzes bei Rechnungen.
     */
    erfassenFuer?: string;
    /** Status statt des aus Fällig/Bezahlt ermittelten, z. B. nicht versendet. */
    status?: WalterZahlungsStatus;
    /** Inhalt der Zusatzspalte, z. B. wie viel einer Rechnung verteilt ist. */
    zusatz?: WalterZahlungsZusatz;
    /** Mietkonto: Summe der Transaktionen hinter der Zeile (ganze Beträge). */
    transaktionen?: number;
    /** Mietkonto: darin enthaltene NK-Vorauszahlung. */
    nkVorauszahlung?: number;
}

/** Tag in der Zusatzspalte der Zahlungsübersicht. */
export interface WalterZahlungsZusatz {
    text: string;
    tag: WalterListItemTagColor;
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
    }
}

/** Eine ganze Transaktion als Eintrag, mit ihrer Zuordnung. */
export function transaktionsEintrag(
    t: WalterMietkontoTransaktion
): WalterZahlungsEintrag {
    return {
        id: t.transaktionId,
        datum: t.datum,
        betrag: t.betrag,
        text: t.zahler ? `Zahlung von ${t.zahler}` : 'Zahlung',
        href: `/transaktionen/${t.transaktionId}`,
        transaktionId: t.transaktionId,
        parts: zuordnungAsParts(t.zuordnung)
    };
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
    const parts = [
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
    const breakdown = parts.length > 1 ? parts : [];
    return {
        id: `monat-${m.jahr}-${m.monat}`,
        periode: formatMonth(m.jahr, m.monat),
        faellig: m.faellig,
        bezahlt: m.bezahlt,
        offen: m.offen,
        breakdown,
        // Ganze Transaktionen; Ausgleiche ohne Transaktion einzeln.
        zahlungen: [
            ...m.transaktionen.map(transaktionsEintrag),
            ...m.zahlungen.filter((z) => !z.transaktionId).map(zahlungsEintrag)
        ],
        erfassenFuer: `${m.jahr}-${String(m.monat).padStart(2, '0')}-01`,
        transaktionen: m.transaktionenBetrag,
        nkVorauszahlung: m.nkVorauszahlung
    };
}

/** Zeilen des Mietkontos, neuester Monat zuerst. Mit `jahr` nur dieses Jahr. */
export function mietkontoZeilen(
    konto: WalterMietkonto,
    jahr?: number
): WalterZahlungsZeile[] {
    return konto.monate
        .filter((m) => jahr === undefined || m.jahr === jahr)
        .sort((a, b) => b.jahr - a.jahr || b.monat - a.monat)
        .map(monatsZeile);
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
    | { art: 'abrechnung'; abrechnungsresultatId: string; erstattung: boolean }
    /** Zahlung einer erfassten Rechnung für Erhaltungsaufwand. */
    | { art: 'erhaltungsaufwand'; buchungssatzId: string; wohnungId: number };
