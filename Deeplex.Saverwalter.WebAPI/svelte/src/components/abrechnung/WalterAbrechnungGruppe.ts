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

import { walter_goto } from '$walter/services/utils';
import type { WalterRechnungEntry } from '$walter/types/WalterBetriebskostenabrechnung.type';

export function goto_or_create_rechnung(
    punkt: WalterRechnungEntry,
    year: number
) {
    if (punkt.rechnungId) {
        walter_goto(`/betriebskostenrechnungen/${punkt.rechnungId}`);
    } else {
        const searchParams = new URLSearchParams();
        searchParams.set('typ', `${punkt.typId}`);
        searchParams.set('umlage', `${punkt.umlageId}`);
        searchParams.set('jahr', `${year}`);
        // TODO betrag

        walter_goto(`/betriebskostenrechnungen/new?${searchParams.toString()}`);
    }
}
