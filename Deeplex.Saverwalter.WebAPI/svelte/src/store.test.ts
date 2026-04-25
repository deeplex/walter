import { get } from 'svelte/store';
import { beforeEach, describe, expect, it } from 'vitest';
import { WalterToastContent } from './lib/WalterToastContent';

import {
    addToast,
    changeTracker,
    openModal,
    removeToast,
    toasts,
    walterModalControl
} from './store';

describe('store helpers', () => {
    beforeEach(() => {
        toasts.set([]);
        changeTracker.set(0);
        walterModalControl.set({
            open: false,
            modalHeading: 'Wieso hat sich das geoffnet?',
            content: 'Bitte einfach schliessen.',
            danger: false,
            primaryButtonText: 'Schliessen',
            submit: async () => true
        });
    });

    it('adds success and failure toasts and can remove them again', () => {
        const toast = new WalterToastContent(
            'Success',
            'Failure',
            (...args: unknown[]) => `ok:${String(args[0] ?? '')}`,
            (...args: unknown[]) => `fail:${String(args[0] ?? '')}`
        );

        addToast(toast, true, 'saved');
        addToast(toast, false, 'broken');

        expect(get(toasts)).toEqual([
            {
                title: 'Failure',
                subtitle: 'fail:broken',
                kind: 'error'
            },
            {
                title: 'Success',
                subtitle: 'ok:saved',
                kind: 'success'
            }
        ]);

        removeToast(0);

        expect(get(toasts)).toEqual([
            {
                title: 'Success',
                subtitle: 'ok:saved',
                kind: 'success'
            }
        ]);
    });

    it('ignores toast additions when the matching title is missing', () => {
        addToast(
            new WalterToastContent(undefined, undefined, () => 'ok', () => 'fail'),
            true
        );
        addToast(
            new WalterToastContent(undefined, undefined, () => 'ok', () => 'fail'),
            false
        );

        expect(get(toasts)).toEqual([]);
    });

    it('opens the modal while preserving existing defaults', async () => {
        const submit = async () => true;

        openModal({
            modalHeading: 'Confirm',
            content: 'Continue?',
            submit
        });

        expect(get(walterModalControl)).toMatchObject({
            open: true,
            modalHeading: 'Confirm',
            content: 'Continue?',
            primaryButtonText: 'Schliessen',
            submit
        });
    });
});