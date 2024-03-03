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

export class WalterMieteEntry extends WalterApiHandler {
    public static ApiURL = `/api/mieten`;

    constructor(
        public id: number,
        public betreffenderMonat: string,
        public zahlungsdatum: string,
        public betrag: number,
        public notiz: string,
        public repeat: number,
        public createdAt: Date,
        public lastModified: Date,
        public vertrag: WalterSelectionEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterMieteEntry) {
        const vertrag =
            json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterMieteEntry(
            json.id,
            json.betreffenderMonat,
            json.zahlungsdatum,
            json.betrag,
            json.notiz,
            json.repeat,
            json.createdAt,
            json.lastModified,
            vertrag,
            permissions
        );
    }
}
