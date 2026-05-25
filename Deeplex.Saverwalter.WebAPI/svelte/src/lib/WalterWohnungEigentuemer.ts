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

export class WalterWohnungEigentuemerEntry extends WalterApiHandler {
    public static ApiURL = `/api/wohnungeigentuemer`;

    constructor(
        public id: number,
        public wohnung: WalterSelectionEntry,
        public kontakt: WalterSelectionEntry,
        public von: string,
        public bis: string | undefined,
        public anteil: number | undefined,
        public createdAt: Date,
        public lastModified: Date,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterWohnungEigentuemerEntry) {
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const kontakt =
            json.kontakt && WalterSelectionEntry.fromJson(json.kontakt);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterWohnungEigentuemerEntry(
            json.id,
            wohnung,
            kontakt,
            json.von,
            json.bis ?? undefined,
            json.anteil ?? undefined,
            json.createdAt,
            json.lastModified,
            permissions
        );
    }
}
