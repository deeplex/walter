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

import { WalterApiHandler } from './WalterApiHandler';
import { WalterBuchungssatzEntry } from './WalterBuchungssatz';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

/** Zuordnung einer Zahlung: wofür sie war (aus den Buchungen, nie aus Freitext). */
export interface WalterZuordnungEintrag {
    text: string;
    href?: string | null;
    betrag: number;
    /** Der Satz dieser Zahlung wurde storniert. */
    storniert: boolean;
}

/** Klartext eines Eintrags der Zuordnung; stornierte Zahlungen als rückgängig gemacht. */
export function zuordnungText(w: WalterZuordnungEintrag): string {
    return w.storniert ? `${w.text} (rückgängig gemacht)` : w.text;
}

/** Zelle „Zuordnung“ einer Transaktionsliste: Klartext oder der Tag „Nicht zugeordnet“. */
export function zuordnungZelle(
    zuordnung: WalterZuordnungEintrag[] | undefined
): string | { text: string; tag: 'warm-gray' } {
    if (!zuordnung || zuordnung.length === 0)
        return { text: 'Nicht zugeordnet', tag: 'warm-gray' };
    return zuordnung.map(zuordnungText).join('; ');
}

export class WalterTransaktionEntry extends WalterApiHandler {
    public static ApiURL = `/api/transaktionen`;

    constructor(
        public id: string,
        public zahler: WalterSelectionEntry,
        public zahlungsempfaenger: WalterSelectionEntry,
        public zahlungsdatum: string,
        public betrag: number,
        public verwendungszweck: string,
        public notiz: string,
        public permissions: WalterPermissions,
        public createdAt: Date,
        public lastModified: Date,
        public buchungssaetze: WalterBuchungssatzEntry[],
        public kannLoeschen: boolean = false,
        public kannStornieren: boolean = false,
        public sperrgrund: string | null = null,
        /** Leer: nicht zugeordnet. */
        public zuordnung: WalterZuordnungEintrag[] = []
    ) {
        super();
    }

    static fromJson(json: WalterTransaktionEntry): WalterTransaktionEntry {
        const zahler =
            json.zahler && WalterSelectionEntry.fromJson(json.zahler);
        const zahlungsEmpfaenger =
            json.zahlungsempfaenger &&
            WalterSelectionEntry.fromJson(json.zahlungsempfaenger);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);
        const buchungssaetze =
            json.buchungssaetze?.map(WalterBuchungssatzEntry.fromJson) ?? [];

        return new WalterTransaktionEntry(
            json.id,
            zahler,
            zahlungsEmpfaenger,
            json.zahlungsdatum,
            json.betrag,
            json.verwendungszweck,
            json.notiz,
            permissions,
            new Date(json.createdAt),
            new Date(json.lastModified),
            buchungssaetze,
            json.kannLoeschen ?? false,
            json.kannStornieren ?? false,
            json.sperrgrund ?? null,
            json.zuordnung ?? []
        );
    }
}

export interface GaragenmietInput {
    garageVertragId: number;
    garageKennung: string;
    betrag: number;
}

export interface StandaloneGaragenmietInput {
    garageVertragId: number;
    garageKennung: string;
    betreffenderMonat: string;
    betrag: number;
}

export interface MietzahlungsInput {
    vertragId?: number;
    betreffenderMonat?: string;
    kaltmiete: number;
    garagen: GaragenmietInput[];
    nkVorauszahlung: number;
    /** Gesamtbetrag statt Aufteilung: der Server teilt in Kaltmiete, Garagen und NK-VZ. */
    gesamtbetrag?: number;
}

export interface BetriebskostenEingangInput {
    existingBuchungssatzId?: string;
    umlageId?: number;
    betreffendesJahr?: number;
    rechnungsDatum?: string;
    betrag: number;
    notiz?: string;
}

export interface ErhaltungsaufwendungsInput {
    existingBuchungssatzId?: string;
    wohnungId?: number;
    betrag: number;
    beschreibung?: string;
}

export interface SonstigerBuchungssatzInput {
    betrag: number;
    beschreibung?: string;
}

export interface NkAnteilEingangInput {
    vertragId?: number;
    umlageId?: number;
    betreffendesJahr: number;
    betrag: number;
    notiz?: string;
}

/**
 * Ausgleich einer abgesendeten NK-Jahresabrechnung: Nachzahlung des Mieters
 * (eingehend) oder Erstattung an den Mieter (ausgehend). Die Richtung ergibt
 * sich aus dem Saldo des Abrechnungsresultats.
 */
export interface AbrechnungsAusgleichInput {
    abrechnungsresultatId?: string;
    betrag: number;
}

export interface TransaktionsInput {
    betrag: number;
    zahlungsdatum: string;
    zahlerId?: number;
    zahlungsempfaengerId?: number;
    verwendungszweck: string;
    notiz?: string;
    mieten: MietzahlungsInput[];
    garagenEingaenge: StandaloneGaragenmietInput[];
    betriebskostenEingaenge: BetriebskostenEingangInput[];
    erhaltungsaufwendungen: ErhaltungsaufwendungsInput[];
    sonstige: SonstigerBuchungssatzInput[];
    nkAnteilEingaenge: NkAnteilEingangInput[];
    abrechnungsAusgleiche: AbrechnungsAusgleichInput[];
}

export function emptyMietzahlungsInput(vertragId?: number): MietzahlungsInput {
    return { kaltmiete: 0, garagen: [], nkVorauszahlung: 0, vertragId };
}

export function emptyTransaktionsInput(): TransaktionsInput {
    return {
        betrag: 0,
        zahlungsdatum: new Date().toISOString().slice(0, 10),
        verwendungszweck: '',
        mieten: [],
        garagenEingaenge: [],
        betriebskostenEingaenge: [],
        erhaltungsaufwendungen: [],
        sonstige: [],
        nkAnteilEingaenge: [],
        abrechnungsAusgleiche: []
    };
}

// ── Vorschau (POST /api/transaktionen/buchen/vorschau) ──────────────────────

export interface VorschauPosition {
    text: string;
    betrag: number;
    danachOffen?: number | null;
}

export interface VorschauZeile {
    kontonummer: string;
    kontobezeichnung: string;
    sollHaben: 'Soll' | 'Haben';
    betrag: number;
}

export interface VorschauSatz {
    buchungsdatum: string;
    buchungsjahr: number;
    beschreibung: string;
    istZahlung: boolean;
    zeilen: VorschauZeile[];
}

export interface TransaktionsVorschau {
    betrag: number;
    verwendungszweck: string;
    zahler?: string | null;
    zahlungsempfaenger?: string | null;
    positionen: VorschauPosition[];
    saetze: VorschauSatz[];
    ausgleiche: number;
}

// ── Rückgängig machen (GET/POST …/rueckgaengig) ─────────────────────────────

export interface BetroffeneAbrechnung {
    jahr: number;
    wohnungIds: number[];
    bezeichnung: string;
    versendet: boolean;
}

export interface RueckgaengigPlan {
    art: 'Loeschen' | 'Storno' | 'Gesperrt';
    sperrgrund?: string | null;
    abrechnungen: BetroffeneAbrechnung[];
}
