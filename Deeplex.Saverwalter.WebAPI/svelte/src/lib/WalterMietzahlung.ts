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

export const WalterMietzahlungApiURL = '/api/mietzahlungen';

export interface WalterMietzahlungInput {
    vertrag: WalterSelectionEntry;
    betreffenderMonat: string;
    zahlungsdatum: string;
    kaltmieteZahlung: number;
    nkZahlung: number;
    notiz?: string;
    permissions?: { read?: boolean; update?: boolean; remove?: boolean };
}

export class WalterMietzahlungListEntry {
    constructor(
        public id: string,
        public buchungsdatum: string,
        public betreffenderMonat: string,
        public kaltmieteZahlung: number,
        public permissions: WalterPermissions
    ) {}

    static fromJson(json: WalterMietzahlungListEntry) {
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);
        return new WalterMietzahlungListEntry(
            json.id,
            json.buchungsdatum,
            json.betreffenderMonat,
            json.kaltmieteZahlung,
            permissions
        );
    }
}

export interface WalterGarageForderungsstatusEntry {
    garageVertragId: number;
    garageKennung: string;
    garagenMiete: number;
    forderungsbetrag: number;
    schonGezahlt: number;
    verbleibendeForderung: number;
    sollstellungVorhanden: boolean;
}

export interface WalterForderungsstatusEntry {
    monat: string;
    forderungsbetrag: number;
    schonGezahlt: number;
    verbleibendeForderung: number;
    nkVorauszahlung: number;
    sollstellungVorhanden: boolean;
    grundmiete: number;
    grundmieteSeit?: string;
    garagen: WalterGarageForderungsstatusEntry[];
}

export interface WalterOffenerPostenStatus {
    rechnungsbetrag: number;
    schonGezahlt: number;
    verbleibenderBetrag: number;
}

export class WalterMietzahlungDetailEntry extends WalterApiHandler {
    public static ApiURL = '/api/mietzahlungen/satz';

    constructor(
        public id: string,
        public buchungsdatum: string,
        public betreffenderMonat: string,
        public betrag: number,
        public vertrag: WalterSelectionEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterMietzahlungDetailEntry) {
        return new WalterMietzahlungDetailEntry(
            json.id,
            json.buchungsdatum,
            json.betreffenderMonat,
            json.betrag,
            WalterSelectionEntry.fromJson(json.vertrag),
            WalterPermissions.fromJson(json.permissions)
        );
    }
}
