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

import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterGarageVertragEntry } from './WalterGarageVertrag';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterGarageEntry extends WalterApiHandler {
    public static ApiURL = `/api/garagen`;

    constructor(
        public id: number,
        public kennung: string,
        public notiz: string | undefined,
        public createdAt: Date,
        public lastModified: Date,
        public besitzer: WalterSelectionEntry | undefined,
        public adresse: WalterAdresseEntry | undefined,
        public aktuelleMieter: string | undefined,
        public vertraege: WalterGarageVertragEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterGarageEntry) {
        const besitzer =
            json.besitzer && WalterSelectionEntry.fromJson(json.besitzer);
        const adresse =
            json.adresse && WalterAdresseEntry.fromJson(json.adresse);
        const vertraege = json.vertraege?.map(
            WalterGarageVertragEntry.fromJson
        );
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterGarageEntry(
            json.id,
            json.kennung,
            json.notiz,
            json.createdAt,
            json.lastModified,
            besitzer,
            adresse,
            json.aktuelleMieter,
            vertraege,
            permissions
        );
    }
}
