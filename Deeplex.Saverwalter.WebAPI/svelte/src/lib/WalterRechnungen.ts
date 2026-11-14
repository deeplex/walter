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

import { formatEuro, formatDate } from '../services/format';
import type {
    WalterZahlungsEintrag,
    WalterZahlungsZeile
} from './WalterMietkonto';
import { ZAHLUNGS_STATUS } from './WalterZahlungsStatus';
import { entityRefHref, type WalterEntityRef } from './WalterEntityRef';

// ── Antworten von GET /api/wohnungen/{id}/erhaltungsaufwand ─────────────────
// und GET /api/umlagen/{id}/rechnungen

/** Zahlung einer Rechnung (Erhaltungsaufwand, Betriebskosten). */
export interface WalterRechnungsZahlung {
    buchungssatzId: string;
    transaktionId?: string | null;
    datum: string;
    betrag: number;
    zahler?: string | null;
    empfaenger?: string | null;
}

export interface WalterErhaltungsaufwandRechnung {
    buchungssatzId: string;
    datum: string;
    jahr: number;
    bezeichnung: string;
    /** Aussteller, bei direkter Zahlung der Empfänger. */
    aussteller?: string | null;
    betrag: number;
    bezahlt: number;
    offen: number;
    storniert: boolean;
    /** Gleich bei der Erfassung bezahlt. */
    direktBezahlt: boolean;
    kannBezahlen: boolean;
    zahlungen: WalterRechnungsZahlung[];
}

export interface WalterWohnungJahr {
    wert: number;
    mieteinnahmen: number;
    erhaltungsaufwand: number;
    /** Betriebskosten, die nicht auf Mieter verteilt wurden (z. B. Leerstand). */
    betriebskostenEigenanteil: number;
}

export interface WalterErhaltungsaufwand {
    rechnungen: WalterErhaltungsaufwandRechnung[];
    jahre: WalterWohnungJahr[];
    vermieterBankkontoId?: number | null;
}

export interface WalterUmlageRechnung {
    buchungssatzId: string;
    jahr: number;
    datum: string;
    betrag: number;
    bezahlt: number;
    /** Auf Mieter bzw. Eigenanteile verteilt. */
    verteilt: number;
    offen: number;
    storniert: boolean;
    kannBezahlen: boolean;
    zahlungen: WalterRechnungsZahlung[];
    /** Wer den verteilten Betrag trägt (Mieteranteil bzw. Eigenanteil), größter zuerst. */
    verteilung: WalterRechnungsVerteilung[];
}

export interface WalterRechnungsVerteilung {
    text: string;
    ref?: WalterEntityRef | null;
    betrag: number;
}

export interface WalterUmlageRechnungen {
    rechnungen: WalterUmlageRechnung[];
    rueckgaengigGemacht: WalterRechnungsZahlung[];
}

// ── Zeilen für WalterZahlungsuebersicht ─────────────────────────────────────

/** Zahlung einer Rechnung: „Zahlung an X“ bzw. „von Y“, sonst nur „Zahlung“. */
export function rechnungsZahlungsEintrag(
    z: WalterRechnungsZahlung
): WalterZahlungsEintrag {
    const parts = [
        z.zahler ? `von ${z.zahler}` : undefined,
        z.empfaenger ? `an ${z.empfaenger}` : undefined
    ].filter(Boolean);
    return {
        id: z.buchungssatzId,
        datum: z.datum,
        betrag: z.betrag,
        text: parts.length > 0 ? `Zahlung ${parts.join(' ')}` : 'Zahlung',
        href: z.transaktionId
            ? `/transaktionen/${z.transaktionId}`
            : `/buchungssaetze/${z.buchungssatzId}`,
        transaktionId: z.transaktionId ?? undefined,
        buchungssatzId: z.buchungssatzId
    };
}

/** Eine Zeile je Rechnung für Erhaltungsaufwand, neueste zuerst. */
export function erhaltungsaufwandZeilen(
    daten: WalterErhaltungsaufwand
): WalterZahlungsZeile[] {
    return daten.rechnungen.map((r) => ({
        id: r.buchungssatzId,
        periode: [formatDate(r.datum), r.bezeichnung, r.aussteller]
            .filter(Boolean)
            .join(' – '),
        href: `/buchungssaetze/${r.buchungssatzId}`,
        faellig: r.betrag,
        bezahlt: r.bezahlt,
        offen: r.offen,
        breakdown: [],
        zahlungen: r.zahlungen.map(rechnungsZahlungsEintrag),
        erfassenFuer: r.kannBezahlen ? r.buchungssatzId : undefined,
        status: r.storniert ? ZAHLUNGS_STATUS.storniert : undefined
    }));
}

/**
 * Eine Zeile je Betriebskostenrechnung, neuestes Jahr zuerst; Zusatzspalte:
 * wie viel auf die Mieter verteilt ist.
 */
export function umlageRechnungZeilen(
    daten: WalterUmlageRechnungen
): WalterZahlungsZeile[] {
    return daten.rechnungen.map((r): WalterZahlungsZeile => {
        const nichtVerteilt = r.betrag - r.verteilt;
        return {
            id: r.buchungssatzId,
            periode: `${r.jahr} – Rechnung vom ${formatDate(r.datum)}`,
            href: `/buchungssaetze/${r.buchungssatzId}`,
            faellig: r.betrag,
            bezahlt: r.bezahlt,
            offen: r.offen,
            breakdown: [],
            zahlungen: r.zahlungen.map(rechnungsZahlungsEintrag),
            sections:
                r.verteilung.length > 0
                    ? [
                          {
                              title: 'Verteilung',
                              items: r.verteilung.map((v, i) => ({
                                  id: `verteilung-${i}`,
                                  betrag: v.betrag,
                                  text: v.text,
                                  href: entityRefHref(v.ref)
                              }))
                          }
                      ]
                    : undefined,
            erfassenFuer: r.kannBezahlen ? r.buchungssatzId : undefined,
            status: r.storniert ? ZAHLUNGS_STATUS.storniert : undefined,
            zusatz: r.storniert
                ? undefined
                : Math.abs(nichtVerteilt) <= 0.005
                  ? { text: 'Verteilt', tag: 'green' }
                  : r.verteilt <= 0.005
                    ? { text: 'Nicht verteilt', tag: 'warm-gray' }
                    : {
                          text: `${formatEuro(r.verteilt)} verteilt`,
                          tag: 'warm-gray'
                      }
        };
    });
}
