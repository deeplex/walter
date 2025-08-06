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

import type { WalterBetriebskostenabrechnungResultatEntry } from '$walter/types';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterBetriebskostenrechnungEntry } from './WalterBetriebskostenrechnung';
import { WalterKontaktEntry } from './WalterKontakt';
import { WalterMieteEntry } from './WalterMiete';
import { WalterMietminderungEntry } from './WalterMietminderung';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVertragVersionEntry } from './WalterVertragVersion';

export class WalterVertragEntry extends WalterApiHandler {
    public static ApiURL = `/api/vertraege`;

    constructor(
        public id: number,
        public beginn: string,
        public ende: string | undefined,
        public notiz: string,
        public mieterAuflistung: string,
        public createdAt: Date,
        public lastModified: Date,
        public wohnung: WalterSelectionEntry,
        public ansprechpartner: WalterSelectionEntry,
        public selectedMieter: WalterSelectionEntry[],
        public versionen: WalterVertragVersionEntry[],
        public mieter: WalterKontaktEntry[],
        public mieten: WalterMieteEntry[],
        public mietminderungen: WalterMietminderungEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[],
        public abrechnungsresultate: WalterBetriebskostenabrechnungResultatEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterVertragEntry) {
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const ansprechpartner =
            json.ansprechpartner &&
            WalterSelectionEntry.fromJson(json.ansprechpartner);
        const selectedMieter = json.selectedMieter?.map(
            WalterSelectionEntry.fromJson
        );
        const versionen = json.versionen?.map(
            WalterVertragVersionEntry.fromJson
        );
        const mieter = json.mieter?.map(WalterKontaktEntry.fromJson);
        const mieten = json.mieten?.map(WalterMieteEntry.fromJson);
        const mietminderungen = json.mietminderungen?.map(
            WalterMietminderungEntry.fromJson
        );
        const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
            WalterBetriebskostenrechnungEntry.fromJson
        );
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        const abrechnungsresultate = json.abrechnungsresultate;

        return new WalterVertragEntry(
            json.id,
            json.beginn,
            json.ende,
            json.notiz,
            json.mieterAuflistung,
            json.createdAt,
            json.lastModified,
            wohnung,
            ansprechpartner,
            selectedMieter,
            versionen,
            mieter,
            mieten,
            mietminderungen,
            betriebskostenrechnungen,
            abrechnungsresultate,
            permissions
        );
    }
}
