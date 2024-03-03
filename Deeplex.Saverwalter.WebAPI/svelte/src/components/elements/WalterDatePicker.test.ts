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

import { expect, describe, it, afterEach } from 'vitest';
import { render, cleanup } from '@testing-library/svelte';
import { WalterDatePicker } from '$walter/components';

describe('WalterDatePicker tests', () => {
    afterEach(cleanup);

    it('should show correct date at startup (15.05.1970)', async () => {
        render(WalterDatePicker, {
            props: { value: '1970-05-15', labelText: 'Datum' }
        });

        const input = document.getElementsByTagName('input').item(0);

        expect(input).toBeDefined();
        expect(input).toHaveProperty('value');
        input && expect(input.value).toBe('15.05.1970');
    });

    it('should show correct date at startup (24.05.2023)', async () => {
        render(WalterDatePicker, {
            props: { value: '2023-05-24', labelText: 'Datum' }
        });

        const input = document.getElementsByTagName('input').item(0);

        expect(input).toBeDefined();
        expect(input).toHaveProperty('value');
        input && expect(input.value).toBe('24.05.2023');
    });

    it('should show correct date at startup (21.09.2020)', async () => {
        render(WalterDatePicker, {
            props: { value: '2020-09-21', labelText: 'Datum' }
        });

        const input = document.getElementsByTagName('input').item(0);

        expect(input).toBeDefined();
        expect(input).toHaveProperty('value');
        input && expect(input.value).toBe('21.09.2020');
    });
});
