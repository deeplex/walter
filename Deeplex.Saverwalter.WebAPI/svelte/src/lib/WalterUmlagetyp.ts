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

export const BETRK_V_NUMMERN: { id: number; text: string }[] = [
    { id: 1, text: 'Nr. 1 - Öffentliche Lasten (Grundsteuer)' },
    { id: 2, text: 'Nr. 2 - Wasserversorgung' },
    { id: 3, text: 'Nr. 3 - Entwässerung' },
    { id: 4, text: 'Nr. 4 - Fahrstuhl' },
    { id: 5, text: 'Nr. 5 - Straßenreinigung und Müllbeseitigung' },
    { id: 6, text: 'Nr. 6 - Gebäudereinigung und Ungezieferbekämpfung' },
    { id: 7, text: 'Nr. 7 - Gartenpflege' },
    { id: 8, text: 'Nr. 8 - Beleuchtung' },
    { id: 9, text: 'Nr. 9 - Schornsteinreinigung' },
    { id: 10, text: 'Nr. 10 - Sach- und Haftpflichtversicherung' },
    { id: 11, text: 'Nr. 11 - Hauswart' },
    { id: 12, text: 'Nr. 12 - Antenne / Breitbandkabel' },
    { id: 13, text: 'Nr. 13 - Maschinelle Wascheinrichtung' },
    { id: 14, text: 'Nr. 14 - Sonstige Betriebskosten' }
];

export class WalterUmlagetypEntry extends WalterApiHandler {
    public static ApiURL = `/api/umlagetypen`;

    constructor(
        public id: number,
        public notiz: string,
        public bezeichnung: string,
        public betrKVNummer: number | null,
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
            json.betrKVNummer ?? null,
            json.createdAt,
            json.lastModified,
            umlagen,
            permissions
        );
    }
}
