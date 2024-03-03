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

import type { WalterSelectionEntry } from '$walter/lib';

export function shouldFilterItem(item: WalterSelectionEntry, value: string) {
    if (!value) return true;

    const text = item.text.toLowerCase();
    const values = `${value}`
        .toLowerCase()
        .split(';')
        .map((e) => e.trim());
    return values.every((val) => text.includes(val));
}
