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

import { expect, describe, it, afterEach, vi } from 'vitest';
import { render, cleanup } from '@testing-library/svelte';
import { createMockFetch } from '$walter/test-helpers/mock-fetch';

import Page from './WalterErhaltungsaufwendungen.svelte';
import { WalterErhaltungsaufwendungEntry } from '$walter/lib';
import { convertDateGerman } from '$walter/services/utils';
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
            new WalterErhaltungsaufwendungEntry(
                seed.toString(),
                seed * 100,
                `2021-${(seed % 12) + 1}-${(seed % 28) + 1}`,
                'Testnotiz',
                `Testbezeichnung ${seed}`,
                new Date(),
                new Date(),
                { id: 1, text: 'Testwohnung' },
                { id: 2, text: 'Testaussteller' },
                [],
                new WalterPermissions(true, true, true)
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
            fetchImpl: createMockFetch()
        });

        const header = document.getElementsByTagName('thead');
        const headers = header.item(0)?.getElementsByTagName('th');

        expect(headers?.item(0)?.innerHTML).toContain('Bezeichnung');
        expect(headers?.item(1)?.innerHTML).toContain('Aussteller');
        expect(headers?.item(2)?.innerHTML).toContain('Wohnung');
        expect(headers?.item(3)?.innerHTML).toContain('Betrag');
        expect(headers?.item(4)?.innerHTML).toContain('Datum');

        expect(headers).toBeDefined();
        expect(headers).toHaveLength(5);
    });

    it('Should have 15 entries', () => {
        render(Page, {
            rows: createEntryMocks(15),
            fetchImpl: createMockFetch()
        });

        const body = document.getElementsByTagName('tbody');
        const rows = body.item(0)?.getElementsByTagName('tr');
        76;
        expect(rows?.length).toBe(15);

        for (let i = 0; i < rows!.length; ++i) {
            const cells = rows?.item(i)?.getElementsByTagName('td');
            expect(cells?.length).toBe(5);
            expect(cells?.item(0)?.innerHTML).toContain(`Testbezeichnung ${i}`);
            expect(cells?.item(1)?.innerHTML).toContain('Testaussteller');
            expect(cells?.item(2)?.innerHTML).toContain('Testwohnung');
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
