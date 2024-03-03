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
import { WalterUmlageEntry } from './WalterUmlage';

export class WalterUmlagetypEntry extends WalterApiHandler {
    public static ApiURL = `/api/umlagetypen`;

    constructor(
        public id: number,
        public notiz: string,
        public bezeichnung: string,
        public createdAt: Date,
        public lastModified: Date,
        public umlagen: WalterUmlageEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterUmlagetypEntry) {
        const umlagen = json.umlagen?.map(WalterUmlageEntry.fromJson);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterUmlagetypEntry(
            json.id,
            json.notiz,
            json.bezeichnung,
            json.createdAt,
            json.lastModified,
            umlagen,
            permissions
        );
    }
}
