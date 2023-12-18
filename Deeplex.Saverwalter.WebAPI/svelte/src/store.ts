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
