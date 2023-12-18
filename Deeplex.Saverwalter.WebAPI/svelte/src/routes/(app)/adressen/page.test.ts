import { expect, describe, it, afterEach, vi, beforeEach } from 'vitest';
import { render, cleanup } from '@testing-library/svelte';
import { writable } from 'svelte/store';

import Page from './+page.svelte';
import { WalterAdresseEntry } from '$walter/lib';

vi.mock('$app/stores', async (importOriginal) => {
    return {
        page: {
            subscribe: writable<boolean>().subscribe,
            url: {
                pathname: 'mock'
            }
        }
    };
});

function createWalterAdresseEntryMocks(entries: number) {
    const mocks = [];
    for (let seed = 0; seed < entries; seed++) {
        mocks.push(
            new WalterAdresseEntry(
                seed,
                `Teststrasse`,
                `${seed}`,
                '12345',
                'Teststadt',
                'Testanschrift',
                'Testnotiz',
                new Date(),
                new Date(),
                [],
                [],
                []
            )
        );
    }

    return mocks;
}

describe('adressen/page.svelte tests', () => {
    afterEach(cleanup);

    it('Should have title Adressen', () => {
        render(Page, {
            data: { rows: createWalterAdresseEntryMocks(5) }
        });

        const title = document.getElementsByTagName('a').item(0)?.innerHTML;

        expect(title?.trim()).toBe('Adressen');
    });

    it('Should have header with 4 entries', () => {
        render(Page, {
            data: { rows: createWalterAdresseEntryMocks(5) }
        });

        const header = document.getElementsByTagName('thead');
        const headers = header.item(0)?.getElementsByTagName('th');

        expect(headers?.item(0)?.innerHTML).toContain('Straße');
        expect(headers?.item(1)?.innerHTML).toContain('Hausnummer');
        expect(headers?.item(2)?.innerHTML).toContain('Postleitzahl');
        expect(headers?.item(3)?.innerHTML).toContain('Stadt');

        expect(headers).toBeDefined();
        expect(headers).toHaveLength(4);
    });

    it('Should have 15 entries', () => {
        render(Page, {
            data: { rows: createWalterAdresseEntryMocks(15) }
        });

        const body = document.getElementsByTagName('tbody');
        const rows = body.item(0)?.getElementsByTagName('tr');

        expect(rows?.length).toBe(15);

        for (let i = 0; i < rows!.length; ++i) {
            const cells = rows?.item(i)?.getElementsByTagName('td');
            expect(cells?.length).toBe(4);
            expect(cells?.item(0)?.innerHTML).toContain('Teststrasse');
            expect(cells?.item(1)?.innerHTML).toContain(`${i}`);
            expect(cells?.item(2)?.innerHTML).toContain('12345');
            expect(cells?.item(3)?.innerHTML).toContain('Teststadt');
        }
    });

    it('Should have a button to create a new entry', () => {
        render(Page, {
            data: { rows: createWalterAdresseEntryMocks(1) }
        });

        const buttons = Array.from(document.getElementsByTagName('button'));

        const addButtons = buttons.filter((e) =>
            e.innerHTML.includes('Eintrag hinzufügen')
        );

        expect(addButtons.length).toBe(1);
        // TODO: this should be true if user has only read rights
        expect(addButtons[0].disabled).toBe(false);
    });
});
