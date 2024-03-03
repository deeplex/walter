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

import { expect, describe, it, afterEach, vi, beforeEach } from 'vitest';
import { render, cleanup } from '@testing-library/svelte';
import { writable } from 'svelte/store';

import Page from './WalterBetriebskostenrechnungen.svelte';
import { WalterBetriebskostenrechnungEntry } from '$walter/lib';
import { convertDateGerman } from '$walter/services/utils';

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

function createEntryMocks(entries: number) {
    const mocks = [];
    for (let seed = 0; seed < entries; seed++) {
        mocks.push(
            new WalterBetriebskostenrechnungEntry(
                seed,
                seed * 100,
                2021,
                `2021-${(seed % 12) + 1}-${(seed % 28) + 1}`,
                'Testnotiz',
                new Date(),
                new Date(),
                { id: 1, text: 'Testtyp' },
                { id: 2, text: 'Testumlage' },
                [],
                []
            )
        );
    }

    return mocks;
}

describe('adressen/page.svelte tests', () => {
    afterEach(cleanup);

    it('Should have header with 4 entries', () => {
        render(Page, {
            rows: createEntryMocks(5),
            fetchImpl: vi.fn()
        });

        const header = document.getElementsByTagName('thead');
        const headers = header.item(0)?.getElementsByTagName('th');

        expect(headers?.item(0)?.innerHTML).toContain('Typ');
        expect(headers?.item(1)?.innerHTML).toContain('Wohnungen');
        expect(headers?.item(2)?.innerHTML).toContain('Betreffendes Jahr');
        expect(headers?.item(3)?.innerHTML).toContain('Betrag');
        expect(headers?.item(4)?.innerHTML).toContain('Datum');

        expect(headers).toBeDefined();
        expect(headers).toHaveLength(5);
    });

    it('Should have 15 entries', () => {
        render(Page, {
            rows: createEntryMocks(15),
            fetchImpl: vi.fn()
        });

        const body = document.getElementsByTagName('tbody');
        const rows = body.item(0)?.getElementsByTagName('tr');

        expect(rows?.length).toBe(15);

        for (let i = 0; i < rows!.length; ++i) {
            const cells = rows?.item(i)?.getElementsByTagName('td');
            expect(cells?.length).toBe(5);
            expect(cells?.item(0)?.innerHTML).toContain('Testtyp');
            // TODO check wohnungen
            // expect(cells?.item(1)?.innerHTML).toContain(`${i}`);
            expect(cells?.item(2)?.innerHTML).toContain('2021');
            expect(cells?.item(3)?.innerHTML).toContain(`${i * 100}`);
            const date = new Date(`2021-${(i % 12) + 1}-${(i % 28) + 1}`);
            expect(cells?.item(4)?.innerHTML).toContain(
                convertDateGerman(date)
            );
        }
    });

    it('Should have a button to create a new entry', () => {
        render(Page, {
            rows: createEntryMocks(1),
            fetchImpl: vi.fn()
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
