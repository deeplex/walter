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

import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterUmlageEntry } from './WalterUmlage';
import { WalterZaehlerstandEntry } from './WalterZaehlerstand';

export class WalterZaehlerEntry extends WalterApiHandler {
    public static ApiURL = `/api/zaehler`;

    constructor(
        public id: number,
        public kennnummer: string,
        public ende: string,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public adresse: WalterAdresseEntry,
        public typ: WalterSelectionEntry | undefined,
        public wohnung: WalterSelectionEntry | undefined,
        public umlagen: WalterUmlageEntry[],
        public selectedUmlagen: WalterSelectionEntry[],
        public staende: WalterZaehlerstandEntry[],
        public lastZaehlerstand: WalterZaehlerstandEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterZaehlerEntry) {
        const adresse =
            json.adresse && WalterAdresseEntry.fromJson(json.adresse);
        const typ = json.typ && WalterSelectionEntry.fromJson(json.typ);
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const umlagen = json.umlagen?.map(WalterUmlageEntry.fromJson);
        const selectedUmlagen = json.selectedUmlagen?.map(
            WalterSelectionEntry.fromJson
        );
        const staende = json.staende?.map(WalterZaehlerstandEntry.fromJson);
        const lastZaehlerstand =
            json.lastZaehlerstand &&
            WalterZaehlerstandEntry.fromJson(json.lastZaehlerstand);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterZaehlerEntry(
            json.id,
            json.kennnummer,
            json.ende,
            json.notiz,
            json.createdAt,
            json.lastModified,
            adresse,
            typ,
            wohnung,
            umlagen,
            selectedUmlagen,
            staende,
            lastZaehlerstand,
            permissions
        );
    }
}
