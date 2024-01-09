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

import { goto } from '$app/navigation';
import { changeTracker, openModal } from '$walter/store';

// Canadian format allegedly is yyyy-mm-dd
export function convertDateCanadian(
    date: Date | undefined
): string | undefined {
    if (date && date.getFullYear) {
        const year = date.getFullYear();
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const day = date.getDate().toString().padStart(2, '0');
        return `${year}-${month}-${day}`;
    } else {
        return undefined;
    }
}

export function convertDateGerman(date: Date | undefined): string | undefined {
    if (date) {
        const year = date.getFullYear();
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const day = date.getDate().toString().padStart(2, '0');

        return `${day}.${month}.${year}`;
    } else {
        return undefined;
    }
}

export function convertTime(
    text: Date | string | undefined
): string | undefined {
    if (text) {
        return new Date(text).toLocaleString('de-DE');
    } else {
        return undefined;
    }
}

export function convertEuro(value: number | undefined): string | undefined {
    return `${(value || 0).toFixed(2)} €`;
}

export function convertPercent(value: number | undefined): string | undefined {
    return `${((value || 0) * 100).toFixed(2)}%`;
}

export function convertM2(value: number | undefined): string | undefined {
    return `${(value || 0).toFixed(2)} m²`;
}

export function convertFixed2(value: number | undefined): string | undefined {
    return `${(value || 0).toFixed(2)}`;
}

let walter_goto_tracker = 0;
changeTracker.subscribe((val) => (walter_goto_tracker = val));

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function walter_goto(url: string, opts: any = undefined) {
    if (walter_goto_tracker > 0) {
        const content = `Bist du sicher, dass du diese Seite verlassen möchtest? Es gibt noch ungespeicherte Änderungen.`;

        openModal({
            modalHeading: 'Seite verlassen?',
            content,
            primaryButtonText: 'Bestätigen',
            submit: () => {
                changeTracker.set(-1);
                changeTracker.set(0);
                goto(url, opts);
            }
        });
    } else {
        return goto(url, opts);
    }
}

export function walter_subscribe_reset_changeTracker(
    updateLastSavedValue: () => void
) {
    changeTracker.subscribe((val) => {
        if (val === -1) {
            updateLastSavedValue();
        }
    });
}

export function walter_update_value<T>(
    last_saved_value: T,
    old_value: T,
    new_value: T
) {
    if (old_value === new_value) {
        return old_value;
    } else if (last_saved_value === old_value) {
        changeTracker.update((val) => val + 1);
    } else if (last_saved_value === new_value) {
        changeTracker.update((val) => val - 1);
    }

    return new_value;
}
