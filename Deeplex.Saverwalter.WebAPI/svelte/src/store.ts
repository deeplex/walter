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

import type { WalterModalControl, WalterToast } from '$walter/types';
import { get, writable, type Writable } from 'svelte/store';
import type { WalterToastContent } from './lib/WalterToastContent';
import type { AuthState } from './services/auth';

export const toasts: Writable<Partial<WalterToast>[]> = writable([]);

export const changeTracker: Writable<number> = writable(0);

export const isWalterSideNavOpen: Writable<boolean> = writable(true);

export function removeToast(index: number) {
    toasts.update((e) => {
        e.splice(index, 1);
        return e;
    });
    return toasts;
}

export function addToast(
    toast: WalterToastContent,
    ok: boolean,
    ...args: unknown[]
) {
    if (ok && toast.successTitle) {
        toasts.update((e) => [
            {
                title: toast.successTitle,
                subtitle: toast.subtitleSuccess(...args),
                kind: 'success'
            },
            ...e
        ]);
    } else if (!ok && toast.failureTitle) {
        toasts.update((e) => [
            {
                title: toast.failureTitle,
                subtitle: toast.subtitleFailure(...args),
                kind: 'error'
            },
            ...e
        ]);
    }
    return toasts;
}

export const walterModalControl: Writable<WalterModalControl> = writable({
    open: false,
    modalHeading: 'Wieso hat sich das geöffnet?',
    content: 'Bitte einfach schließen.',
    danger: false,
    primaryButtonText: 'Schließen',
    submit: async () => true
});

export function openModal(configs: Partial<WalterModalControl>) {
    walterModalControl.update(() => {
        return {
            ...get(walterModalControl),
            ...configs,
            open: true
        };
    });
}
