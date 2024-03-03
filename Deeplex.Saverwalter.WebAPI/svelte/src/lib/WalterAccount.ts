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
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVerwalterEntry } from './WalterVerwalter';

export class WalterAccountEntry extends WalterApiHandler {
    public static ApiURL = `/api/accounts`;
    public static ApiURLUser = `/api/user`;

    constructor(
        public id: number,
        public username: string,
        public name: string,
        public role: WalterSelectionEntry,
        public createdAt: Date,
        public lastModified: Date,
        public resetToken: string,
        public resetTokenExpires: Date,
        public verwalter: WalterVerwalterEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterAccountEntry): WalterAccountEntry {
        const verwalter = json.verwalter?.map(WalterVerwalterEntry.fromJson);
        const role = WalterSelectionEntry.fromJson(json.role);

        return new WalterAccountEntry(
            json.id,
            json.username,
            json.name,
            role,
            json.createdAt,
            json.lastModified,
            json.resetToken,
            json.resetTokenExpires,
            verwalter
        );
    }
}
