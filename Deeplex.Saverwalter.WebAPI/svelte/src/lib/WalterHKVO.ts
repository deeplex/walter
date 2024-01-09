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

export class WalterHKVOEntry extends WalterApiHandler {
    // public static ApiURL = `/api/hkvo`;

    constructor(
        public id: number,
        public notiz: string,
        public hkvO_P7: number,
        public hkvO_P8: number,
        public hkvO_P9: WalterSelectionEntry,
        public strompauschale: number,
        public stromrechnung: WalterSelectionEntry,
        public createdAt: Date,
        public lastModified: Date,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterHKVOEntry) {
        const hkvo_p9 =
            json.hkvO_P9 && WalterSelectionEntry.fromJson(json.hkvO_P9);
        const stromrechnung =
            json.stromrechnung &&
            WalterSelectionEntry.fromJson(json.stromrechnung);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterHKVOEntry(
            json.id,
            json.notiz,
            json.hkvO_P7,
            json.hkvO_P8,
            hkvo_p9,
            json.strompauschale,
            stromrechnung,
            json.createdAt,
            json.lastModified,
            permissions
        );
    }
}
