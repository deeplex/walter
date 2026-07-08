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
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterVertragsNkAnteilEntry extends WalterApiHandler {
    public static ApiURL = `/api/vertrags-nk-anteile`;

    constructor(
        public id: string,
        public betrag: number,
        public datum: string,
        public betreffendesJahr: number,
        public notiz: string | undefined,
        public vertrag: WalterSelectionEntry,
        public umlage: WalterSelectionEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(
        json: WalterVertragsNkAnteilEntry
    ): WalterVertragsNkAnteilEntry {
        return new WalterVertragsNkAnteilEntry(
            json.id,
            json.betrag,
            json.datum,
            json.betreffendesJahr,
            json.notiz,
            WalterSelectionEntry.fromJson(json.vertrag),
            WalterSelectionEntry.fromJson(json.umlage),
            WalterPermissions.fromJson(json.permissions)
        );
    }
}

export function validateVertragsNkAnteil(e: unknown): boolean {
    const entry = e as Partial<WalterVertragsNkAnteilEntry>;
    return !!(
        entry.vertrag?.id &&
        entry.umlage?.id &&
        entry.betrag &&
        entry.betrag > 0
    );
}
