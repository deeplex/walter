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

import type {
    WalterZahlungsEintrag,
    WalterZahlungsZeile
} from './WalterMietkonto';
import { ZAHLUNGS_STATUS } from './WalterZahlungsStatus';

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
}

export interface WalterUmlageRechnungen {
    rechnungen: WalterUmlageRechnung[];
    rueckgaengigGemacht: WalterRechnungsZahlung[];
}

// ── Zeilen für WalterZahlungsuebersicht ─────────────────────────────────────

const eur = (v: number) =>
    v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
const datumText = (d: string) => new Date(d).toLocaleDateString('de-DE');

/** Zahlung einer Rechnung: „Zahlung an X“ bzw. „von Y“, sonst nur „Zahlung“. */
export function rechnungsZahlungsEintrag(
    z: WalterRechnungsZahlung
): WalterZahlungsEintrag {
    const teile = [
        z.zahler ? `von ${z.zahler}` : undefined,
        z.empfaenger ? `an ${z.empfaenger}` : undefined
    ].filter(Boolean);
    return {
        id: z.buchungssatzId,
        datum: z.datum,
        betrag: z.betrag,
        text: teile.length > 0 ? `Zahlung ${teile.join(' ')}` : 'Zahlung',
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
        periode: [datumText(r.datum), r.bezeichnung, r.aussteller]
            .filter(Boolean)
            .join(' – '),
        href: `/buchungssaetze/${r.buchungssatzId}`,
        faellig: r.betrag,
        bezahlt: r.bezahlt,
        offen: r.offen,
        aufschluesselung: [],
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
            periode: `${r.jahr} – Rechnung vom ${datumText(r.datum)}`,
            href: `/buchungssaetze/${r.buchungssatzId}`,
            faellig: r.betrag,
            bezahlt: r.bezahlt,
            offen: r.offen,
            aufschluesselung: [],
            zahlungen: r.zahlungen.map(rechnungsZahlungsEintrag),
            erfassenFuer: r.kannBezahlen ? r.buchungssatzId : undefined,
            status: r.storniert ? ZAHLUNGS_STATUS.storniert : undefined,
            zusatz: r.storniert
                ? undefined
                : Math.abs(nichtVerteilt) <= 0.005
                  ? { text: 'Verteilt', tag: 'green' }
                  : r.verteilt <= 0.005
                    ? { text: 'Nicht verteilt', tag: 'warm-gray' }
                    : {
                          text: `${eur(r.verteilt)} verteilt`,
                          tag: 'warm-gray'
                      }
        };
    });
}
