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

import { convertDateCanadian } from '$walter/services/utils';

export function getDateForMonthPicker(date: string | undefined) {
    if (!date) {
        return undefined;
    }

    const parsed = new Date(date);
    if (Number.isNaN(parsed.getTime())) {
        return undefined;
    }

    const month = `${parsed.getMonth() + 1}`.padStart(2, '0');
    return `${month}.${parsed.getFullYear()}`;
}

export function parseMonthPickerValue(value: string | undefined) {
    if (!value) {
        return undefined;
    }

    const match = /^(\d{1,2})\.(\d{4})$/.exec(value.trim());
    if (!match) {
        return undefined;
    }

    const month = +match[1];
    const year = +match[2];
    if (month < 1 || month > 12) {
        return undefined;
    }

    return convertDateCanadian(new Date(year, month - 1, 1));
}
