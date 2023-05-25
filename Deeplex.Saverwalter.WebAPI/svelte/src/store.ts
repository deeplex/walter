import type { WalterModalControl, WalterToast } from '$walter/types';
import { get, writable, type Writable } from 'svelte/store';
import type { WalterToastContent } from './lib/WalterToastContent';

export const toasts: Writable<Partial<WalterToast>[]> = writable([]);

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
    const title = ok ? toast.successTitle : toast.failureTitle;
    const subtitle = ok
        ? toast.subtitleSuccess(...args)
        : toast.subtitleFailure(...args);

    toasts.update((e) => [
        { title, subtitle, kind: ok ? 'success' : 'error' },
        ...e
    ]);
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
