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

export class WalterZaehlerstandEntry extends WalterApiHandler {
    public static ApiURL = `/api/zaehlerstaende`;

    constructor(
        public id: number,
        public stand: number,
        public datum: string,
        public einheit: string,
        public zaehler: WalterSelectionEntry,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterZaehlerstandEntry) {
        const zaehler =
            json.zaehler && WalterSelectionEntry.fromJson(json.zaehler);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterZaehlerstandEntry(
            json.id,
            json.stand,
            json.datum,
            json.einheit,
            zaehler,
            json.notiz,
            json.createdAt,
            json.lastModified,
            permissions
        );
    }
}
