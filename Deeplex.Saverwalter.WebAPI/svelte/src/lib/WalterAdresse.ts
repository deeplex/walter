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
import { WalterKontaktEntry } from './WalterKontakt';
import { WalterPermissions } from './WalterPermissions';
import { WalterWohnungEntry } from './WalterWohnung';
import { WalterZaehlerEntry } from './WalterZaehler';

export class WalterAdresseEntry extends WalterApiHandler {
    public static ApiURL = `/api/adressen`;

    constructor(
        public id: number,
        public strasse: string,
        public hausnummer: string,
        public postleitzahl: string,
        public stadt: string,
        public anschrift: string,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public wohnungen: WalterWohnungEntry[],
        public kontakte: WalterKontaktEntry[],
        public zaehler: WalterZaehlerEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterAdresseEntry): WalterAdresseEntry {
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const kontakte = json.kontakte?.map(WalterKontaktEntry.fromJson);
        const zaehler = json.zaehler?.map(WalterZaehlerEntry.fromJson);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterAdresseEntry(
            json.id,
            json.strasse,
            json.hausnummer,
            json.postleitzahl,
            json.stadt,
            json.anschrift,
            json.notiz,
            json.createdAt,
            json.lastModified,
            wohnungen,
            kontakte,
            zaehler,
            permissions
        );
    }
}
