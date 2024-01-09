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
import { WalterMieteEntry } from './WalterMiete';

export class WalterMiettabelleEntry extends WalterApiHandler {
    public static ApiURL = `/api/miettabelle`;

    constructor(
        public id: number,
        public bezeichnung: string,
        public createdAt: Date,
        public lastModified: Date,
        public mieten: WalterMieteEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterMiettabelleEntry) {
        const mieten =
            json.mieten && json.mieten.map(WalterMieteEntry.fromJson);

        return new WalterMiettabelleEntry(
            json.id,
            json.bezeichnung,
            json.createdAt,
            json.lastModified,
            mieten
        );
    }
}
