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
import { WalterBetriebskostenrechnungEntry } from './WalterBetriebskostenrechnung';
import { WalterHKVOEntry } from './WalterHKVO';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterWohnungEntry } from './WalterWohnung';
import { WalterZaehlerEntry } from './WalterZaehler';

export class WalterUmlageEntry extends WalterApiHandler {
    public static ApiURL = `/api/umlagen`;

    constructor(
        public id: number,
        public notiz: string,
        public beschreibung: string,
        public wohnungenBezeichnung: string,
        public hKVO: Partial<WalterHKVOEntry>,
        public createdAt: Date,
        public lastModified: Date,
        public zaehler: WalterZaehlerEntry[],
        public selectedZaehler: WalterSelectionEntry[],
        public typ: WalterSelectionEntry,
        public schluessel: WalterSelectionEntry,
        public selectedWohnungen: WalterSelectionEntry[],
        public wohnungen: WalterWohnungEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterUmlageEntry): WalterUmlageEntry {
        const typ = json.typ && WalterSelectionEntry.fromJson(json.typ);
        const schluessel =
            json.schluessel && WalterSelectionEntry.fromJson(json.schluessel);
        const selectedWohnungen = json.selectedWohnungen?.map(
            WalterSelectionEntry.fromJson
        );
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
            WalterBetriebskostenrechnungEntry.fromJson
        );
        const selectedZaehler = json.selectedZaehler?.map(
            WalterSelectionEntry.fromJson
        );
        const hkvo =
            json.hKVO && WalterHKVOEntry.fromJson(json.hKVO as WalterHKVOEntry);
        const zaehler = json.zaehler?.map(WalterZaehlerEntry.fromJson);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterUmlageEntry(
            json.id,
            json.notiz,
            json.beschreibung,
            json.wohnungenBezeichnung,
            hkvo,
            json.createdAt,
            json.lastModified,
            zaehler,
            selectedZaehler,
            typ,
            schluessel,
            selectedWohnungen,
            wohnungen,
            betriebskostenrechnungen,
            permissions
        );
    }
}
