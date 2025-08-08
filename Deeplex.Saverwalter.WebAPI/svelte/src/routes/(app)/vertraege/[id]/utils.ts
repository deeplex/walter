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

import type {
    WalterVertragEntry,
} from '$walter/lib';
import { convertDateCanadian } from '$walter/services/utils';

export function getMietminderungEntry(vertrag: WalterVertragEntry) {
    const today = new Date();
    return {
        vertrag: { id: vertrag.id, text: '' },
        beginn: convertDateCanadian(today),
        ende: convertDateCanadian(
            new Date(today.setMonth(today.getMonth() + 1))
        ),
        permissions: vertrag.permissions
    };
}

export function getVertragversionEntry(vertrag: WalterVertragEntry) {
    return {
        vertrag: { id: vertrag.id, text: '' },
        beginn: convertDateCanadian(new Date()),
        personenzahl:
            vertrag.versionen[vertrag.versionen.length - 1]?.personenzahl,
        grundmiete: vertrag.versionen[vertrag.versionen.length - 1]?.grundmiete,
        permissions: vertrag.permissions
    };
}

export function getMieteEntry(vertrag: WalterVertragEntry) {
    return {
        vertrag: { id: vertrag.id, text: '' },
        zahlungsdatum: convertDateCanadian(new Date()),
        betrag:
            vertrag.versionen[vertrag.versionen.length - 1]?.grundmiete || 0,
        permissions: vertrag.permissions
    };
}
