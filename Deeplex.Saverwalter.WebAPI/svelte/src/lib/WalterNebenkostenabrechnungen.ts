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

// ── Antwort von GET /api/vertraege/{id}/abrechnungen ────────────────────────

export type WalterAbrechnungsStatus = 'Entwurf' | 'Abgesendet' | 'Storniert';

export interface WalterAbrechnungsZahlung {
    buchungssatzId: string;
    transaktionId?: string | null;
    datum: string;
    betrag: number;
    zahler?: string | null;
    empfaenger?: string | null;
    /** Erstattung an den Mieter (sonst Nachzahlung des Mieters). */
    erstattung: boolean;
}

export interface WalterNebenkostenabrechnung {
    abrechnungsresultatId: string;
    jahr: number;
    status: WalterAbrechnungsStatus;
    /** Positiv: Nachzahlung des Mieters; negativ: Erstattung an den Mieter. */
    saldo: number;
    betrag: number;
    bezahlt: number;
    offen: number;
    kannAusgleichen: boolean;
    zahlungen: WalterAbrechnungsZahlung[];
}

export interface WalterNebenkostenabrechnungen {
    abrechnungen: WalterNebenkostenabrechnung[];
    rueckgaengigGemacht: WalterAbrechnungsZahlung[];
    vermieterBankkontoId?: number | null;
}

// ── Zeilen für WalterZahlungsuebersicht ─────────────────────────────────────

/** Nachzahlung („von“ dem Mieter) oder Erstattung („an“ den Mieter). */
export function abrechnungsZahlungsEintrag(
    z: WalterAbrechnungsZahlung
): WalterZahlungsEintrag {
    const erstattung = z.erstattung;
    const art = erstattung ? 'Erstattung' : 'Nachzahlung';
    const wer = erstattung ? z.empfaenger : z.zahler;
    return {
        id: z.buchungssatzId,
        datum: z.datum,
        betrag: z.betrag,
        text: wer ? `${art} – ${erstattung ? 'an' : 'von'} ${wer}` : art,
        href: z.transaktionId
            ? `/transaktionen/${z.transaktionId}`
            : `/buchungssaetze/${z.buchungssatzId}`,
        transaktionId: z.transaktionId ?? undefined,
        buchungssatzId: z.buchungssatzId
    };
}

function art(a: WalterNebenkostenabrechnung): string {
    if (a.saldo > 0) return 'Nachzahlung';
    if (a.saldo < 0) return 'Erstattung';
    return 'ohne Saldo';
}

/** Eine Zeile je Abrechnung, wie vom Server geliefert (neuestes Jahr zuerst). */
export function abrechnungsZeilen(
    daten: WalterNebenkostenabrechnungen
): WalterZahlungsZeile[] {
    return daten.abrechnungen.map((a) => ({
        id: a.abrechnungsresultatId,
        periode: `Abrechnung ${a.jahr} – ${art(a)}`,
        href: `/abrechnungsresultate/${a.abrechnungsresultatId}`,
        faellig: a.betrag,
        bezahlt: a.bezahlt,
        offen: a.offen,
        aufschluesselung: [],
        zahlungen: a.zahlungen.map(abrechnungsZahlungsEintrag),
        erfassenFuer: a.kannAusgleichen ? a.abrechnungsresultatId : undefined,
        status:
            a.status === 'Storniert'
                ? ZAHLUNGS_STATUS.storniert
                : a.status === 'Entwurf'
                  ? ZAHLUNGS_STATUS.nichtVersendet
                  : undefined
    }));
}
