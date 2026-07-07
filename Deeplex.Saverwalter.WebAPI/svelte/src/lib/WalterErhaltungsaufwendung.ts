// Copyright (c) 2023-2024 Kai Lawrence
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

export interface EABuchungszeileInfo {
    konto: string;
    sollHaben: string;
    betrag: number;
}

export class WalterErhaltungsaufwendungEntry extends WalterApiHandler {
    public static ApiURL = `/api/erhaltungsaufwendungen`;
    public static ApiURLId(id: string) {
        return `${WalterErhaltungsaufwendungEntry.ApiURL}/${id}`;
    }

    constructor(
        public id: string,
        public betrag: number,
        public datum: string,
        public notiz: string,
        public bezeichnung: string,
        public createdAt: Date,
        public lastModified: Date,
        public wohnung: WalterSelectionEntry,
        public aussteller: WalterSelectionEntry | undefined,
        public buchungszeilen: EABuchungszeileInfo[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterErhaltungsaufwendungEntry) {
        const wohnung = WalterSelectionEntry.fromJson(json.wohnung);
        const aussteller = json.aussteller
            ? WalterSelectionEntry.fromJson(json.aussteller)
            : undefined;
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterErhaltungsaufwendungEntry(
            json.id,
            json.betrag,
            json.datum,
            json.notiz,
            json.bezeichnung,
            json.createdAt,
            json.lastModified,
            wohnung,
            aussteller,
            json.buchungszeilen ?? [],
            permissions
        );
    }
}
