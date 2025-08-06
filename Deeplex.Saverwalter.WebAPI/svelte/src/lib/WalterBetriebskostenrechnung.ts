// Copyright (c) 2023-2025 Kai Lawrence
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
import { WalterWohnungEntry } from './WalterWohnung';

export class WalterBetriebskostenrechnungEntry extends WalterApiHandler {
    public static ApiURL = `/api/betriebskostenrechnungen`;

    constructor(
        public id: number,
        public betrag: number,
        public betreffendesJahr: number,
        public datum: string,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public typ: WalterSelectionEntry,
        public umlage: WalterSelectionEntry,
        public wohnungen: WalterWohnungEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[],
        public permissions: WalterPermissions,
    ) {
        super();
    }

    public static fromJson(
        json: WalterBetriebskostenrechnungEntry
    ): WalterBetriebskostenrechnungEntry {
        const typ = json.typ && WalterSelectionEntry.fromJson(json.typ);
        const umlage =
            json.umlage && WalterSelectionEntry.fromJson(json.umlage);
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
            WalterBetriebskostenrechnungEntry.fromJson
        );
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterBetriebskostenrechnungEntry(
            json.id,
            json.betrag,
            json.betreffendesJahr,
            json.datum,
            json.notiz,
            json.createdAt,
            json.lastModified,
            typ,
            umlage,
            wohnungen,
            betriebskostenrechnungen,
            permissions);
    }
}
