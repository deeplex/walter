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

import { WalterApiHandler } from './WalterApiHandler';
import type { WalterBuchungskontoEntry } from './WalterBuchungskonto';
import { WalterGarageVertragVersionEntry } from './WalterGarageVertragVersion';
import { WalterKontaktEntry } from './WalterKontakt';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterGarageVertragEntry extends WalterApiHandler {
    public static ApiURL = `/api/garage-vertraege`;

    constructor(
        public id: number,
        public beginn: string,
        public ende: string | undefined,
        public notiz: string | undefined,
        public mieterAuflistung: string | undefined,
        public createdAt: Date,
        public lastModified: Date,
        public garage: WalterSelectionEntry,
        public vertrag: WalterSelectionEntry | undefined,
        public selectedMieter: WalterSelectionEntry[],
        public mieter: WalterKontaktEntry[],
        public versionen: WalterGarageVertragVersionEntry[],
        public permissions: WalterPermissions,
        public konten: Partial<WalterBuchungskontoEntry>[] = []
    ) {
        super();
    }

    get aktuelleGaragenMiete(): number | undefined {
        if (!this.versionen || this.versionen.length === 0) return undefined;
        const today = new Date().toISOString().slice(0, 10);
        const active = this.versionen
            .filter((v) => v.beginn <= today)
            .sort((a, b) => b.beginn.localeCompare(a.beginn));
        return active[0]?.garagenMiete ?? this.versionen[0]?.garagenMiete;
    }

    static fromJson(json: WalterGarageVertragEntry) {
        const garage =
            json.garage && WalterSelectionEntry.fromJson(json.garage);
        const vertrag =
            json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);
        const selectedMieter = json.selectedMieter?.map(
            WalterSelectionEntry.fromJson
        );
        const mieter = json.mieter?.map(WalterKontaktEntry.fromJson);
        const versionen = json.versionen?.map(
            WalterGarageVertragVersionEntry.fromJson
        );
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterGarageVertragEntry(
            json.id,
            json.beginn,
            json.ende,
            json.notiz,
            json.mieterAuflistung,
            json.createdAt,
            json.lastModified,
            garage,
            vertrag,
            selectedMieter,
            mieter,
            versionen,
            permissions,
            json.konten ?? []
        );
    }
}
