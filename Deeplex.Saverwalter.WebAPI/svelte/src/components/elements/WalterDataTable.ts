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

import { formatDate } from '$walter/services/format';

export function dates(key: string) {
    switch (key) {
        case 'beginn':
        case 'ende':
        case 'datum':
        case 'betreffenderMonat':
        case 'zahlungsdatum':
        case 'buchungsdatum':
        case 'lastZaehlerstand.datum':
            return true;
        default:
            return false;
    }
}

export function time(key: string) {
    switch (key) {
        case 'creationTime':
        case 'LastModified':
            return true;
        default:
            return false;
    }
}

/** Mengen wie Zählerstände: deutsch, bis zu drei Nachkommastellen. */
export function zahlen(key: string) {
    switch (key) {
        case 'stand':
        case 'lastZaehlerstand.stand':
            return true;
        default:
            return false;
    }
}

export function euro(key: string) {
    switch (key) {
        case 'betrag':
        case 'grundmiete':
        case 'kosten':
        case 'gesamtBetrag':
        case 'betragLetztesJahr':
        case 'saldo':
        case 'kontoSoll':
        case 'kontoHaben':
            return true;
        default:
            return false;
    }
}

export function formatToTableDate(date: string) {
    return formatDate(date);
}
