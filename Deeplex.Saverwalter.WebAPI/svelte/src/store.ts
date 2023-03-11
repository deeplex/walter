import type { WalterModalControl, WalterToast } from '$WalterTypes';
import { get, writable, type Writable } from 'svelte/store';

export const toasts: Writable<Partial<WalterToast>[]> = writable([]);

export function removeToast(index: number) {
    toasts.update(e => {
        e.splice(index, 1);
        return e;
    });
    return toasts;
}

export function addToast(toast: Partial<WalterToast>) {
    toasts.update((e) => [...e, toast]);
    return toasts;
}

export const walterModalControl: Writable<WalterModalControl> = writable({
    open: false,
    modalHeading: "Wieso hat sich das geöffnet?",
    content: "Bitte einfach schließen.",
    danger: false,
    primaryButtonText: "Schließen",
    submit: async () => true,
});

export function openModal(configs: Partial<WalterModalControl>) {
    walterModalControl.update(() => {
        return {
            ...get(walterModalControl),
            ...configs,
            open: true
        }
    });
}
