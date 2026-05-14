// Copyright (c) 2023-2025 Kai Lawrence
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

export class WalterAbrechnungsresultatEntry extends WalterApiHandler {
    public static ApiURL = `/api/abrechnungsresultate`;
    public static ApiURLId(id: string) {
        return `${WalterAbrechnungsresultatEntry.ApiURL}/${id}`;
    }
    constructor(
        public id: string,
        public vertrag: WalterSelectionEntry,
        public jahr: number,
        public abgesendet: boolean,
        public saldo: number,
        public notiz: string,
        public buchungssatzId: string,
        public nkKontoZeilen: NkKontoZeileInfo[],
        public createdAt: Date,
        public lastModified: Date,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterAbrechnungsresultatEntry) {
        const vertrag =
            json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterAbrechnungsresultatEntry(
            json.id,
            vertrag,
            json.jahr,
            json.abgesendet,
            json.saldo,
            json.notiz,
            json.buchungssatzId,
            json.nkKontoZeilen ?? [],
            json.createdAt,
            json.lastModified,
            permissions
        );
    }
}

export type NkKontoZeileInfo = {
    datum: string;
    beschreibung: string;
    istSoll: boolean;
    betrag: number;
}
