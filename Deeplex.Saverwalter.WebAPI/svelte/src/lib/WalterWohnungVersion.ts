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

export class WalterWohnungVersionEntry extends WalterApiHandler {
    public static ApiURL = `/api/wohnungversionen`;

    constructor(
        public id: number,
        public beginn: string,
        public wohnflaeche: number,
        public nutzflaeche: number,
        public miteigentumsanteile: number,
        public einheiten: number,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public wohnung: WalterSelectionEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterWohnungVersionEntry) {
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterWohnungVersionEntry(
            json.id,
            json.beginn,
            json.wohnflaeche,
            json.nutzflaeche,
            json.miteigentumsanteile,
            json.einheiten,
            json.notiz,
            json.createdAt,
            json.lastModified,
            wohnung,
            permissions
        );
    }
}
