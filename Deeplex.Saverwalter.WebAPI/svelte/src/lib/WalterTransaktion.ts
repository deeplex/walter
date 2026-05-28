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
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

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
        public lastModified: Date
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
            new Date(json.lastModified)
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
        nkAnteilEingaenge: []
    };
}
