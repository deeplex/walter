// Copyright (c) 2023-2026 Kai Lawrence
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

import { afterEach, describe, expect, it } from 'vitest';
import { cleanup, render } from '@testing-library/svelte';

import WalterAdresse from './WalterAdresse.svelte';
import { WalterPermissions } from '$walter/lib/WalterPermissions';

function entryWith(permissions: WalterPermissions) {
    return {
        id: 1,
        strasse: 'Teststraße',
        hausnummer: '1',
        postleitzahl: '12345',
        stadt: 'Teststadt',
        permissions
    };
}

function inputs(container: HTMLElement): HTMLInputElement[] {
    return Array.from(container.querySelectorAll('input.bx--text-input'));
}

describe('WalterAdresse permission gating', () => {
    afterEach(cleanup);

    it('renders read-only fields when the user lacks update permission', () => {
        const { container } = render(WalterAdresse, {
            entry: entryWith(new WalterPermissions(true, false, false))
        });

        const fields = inputs(container);
        expect(fields.length).toBeGreaterThan(0);
        for (const field of fields) {
            expect(field.readOnly).toBe(true);
        }
    });

    it('renders editable fields when the user has update permission', () => {
        const { container } = render(WalterAdresse, {
            entry: entryWith(new WalterPermissions(true, true, true))
        });

        const fields = inputs(container);
        expect(fields.length).toBeGreaterThan(0);
        for (const field of fields) {
            expect(field.readOnly).toBe(false);
        }
    });
});
