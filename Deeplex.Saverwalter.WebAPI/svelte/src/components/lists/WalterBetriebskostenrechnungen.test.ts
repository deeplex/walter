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
import { createMockFetch } from '$walter/test-helpers/mock-fetch';

import Page from './WalterBetriebskostenrechnungen.svelte';
import { WalterBetriebskostenrechnungEntry } from '$walter/lib';
import { WalterPermissions } from '$walter/lib/WalterPermissions';

vi.mock('$app/stores', async () => {
    const { readable } = await import('svelte/store');
    return {
        page: readable({ url: new URL('http://localhost/mock'), params: {} })
    };
});

function createEntryMocks(entries: number) {
    const mocks = [];
    for (let seed = 0; seed < entries; seed++) {
        mocks.push(
            new WalterBetriebskostenrechnungEntry(
                seed.toString(),
                seed * 100, // betrag
                seed * 100, // verteilt
                2021, // betreffendesJahr
                `2021-${(seed % 12) + 1}-${(seed % 28) + 1}`,
                'Testnotiz',
                new Date(),
                new Date(),
                { id: 1, text: 'Testtyp' },
                { id: 2, text: 'Testumlage' },
                [],
                [],
                new WalterPermissions(true, true, true)
            )
        );
    }

    return mocks;
}

describe('adressen/page.svelte tests', () => {
    afterEach(cleanup);

    it('Should have header with 6 entries', () => {
        render(Page, {
            rows: createEntryMocks(5),
            fetchImpl: createMockFetch()
        });

        const header = document.getElementsByTagName('thead');
        const headers = header.item(0)?.getElementsByTagName('th');

        expect(headers?.item(0)?.innerHTML).toContain('Typ');
        expect(headers?.item(1)?.innerHTML).toContain('Wohnungen');
        expect(headers?.item(2)?.innerHTML).toContain('Betreffendes Jahr');
        expect(headers?.item(3)?.innerHTML).toContain('Betrag');
        expect(headers?.item(4)?.innerHTML).toContain('Datum');
        // Column added by the proper-books work: a balanced (ausgeglichen) flag.
        expect(headers?.item(5)?.innerHTML).toContain('⚖');

        expect(headers).toBeDefined();
        expect(headers).toHaveLength(6);
    });

    it('Should have 15 entries', () => {
        render(Page, {
            rows: createEntryMocks(15),
            fetchImpl: createMockFetch()
        });

        const body = document.getElementsByTagName('tbody');
        const rows = body.item(0)?.getElementsByTagName('tr');

        expect(rows?.length).toBe(15);

        for (let i = 0; i < rows!.length; ++i) {
            const cells = rows?.item(i)?.getElementsByTagName('td');
            expect(cells?.length).toBe(6);
            expect(cells?.item(0)?.innerHTML).toContain('Testtyp');
            // TODO check wohnungen
            // expect(cells?.item(1)?.innerHTML).toContain(`${i}`);
            expect(cells?.item(2)?.innerHTML).toContain('2021');
            expect(cells?.item(3)?.innerHTML).toMatch(/\d/);
            expect(cells?.item(4)?.innerHTML).toMatch(/\d{2}\.\d{2}\.\d{4}/);
            // Balanced flag column renders either ✓ or ✗.
            expect(cells?.item(5)?.innerHTML).toMatch(/[✓✗]/);
        }
    });

    it('Should have a button to create a new entry', () => {
        render(Page, {
            rows: createEntryMocks(1),
            fetchImpl: createMockFetch()
        });

        // The "create new entry" affordance is a link to the entity's /new page.
        const addLinks = Array.from(document.querySelectorAll('a')).filter(
            (e) => e.textContent?.includes('Eintrag hinzufügen')
        );

        expect(addLinks.length).toBe(1);
        expect(addLinks[0].getAttribute('href')).toContain('/new');
    });
});
