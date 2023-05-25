import { expect, describe, it, afterEach } from 'vitest';
import { render, cleanup } from '@testing-library/svelte';
import { WalterDatePicker } from '$WalterComponents';

describe('WalterDatePicker tests', () => {
    afterEach(cleanup);

    it('should show correct date at startup (15.05.1970)', async () => {
        render(WalterDatePicker, {
            props: { value: '1970-05-15', labelText: 'Datum' }
        });

        const input = document.getElementsByTagName('input').item(0);

        expect(input).toBeDefined();
        expect(input).toHaveProperty('value');
        expect(input!.value).toBe('15.05.1970');
    });

    it('should show correct date at startup (24.05.2023)', async () => {
        render(WalterDatePicker, {
            props: { value: '2023-05-24', labelText: 'Datum' }
        });

        const input = document.getElementsByTagName('input').item(0);

        expect(input).toBeDefined();
        expect(input).toHaveProperty('value');
        expect(input!.value).toBe('24.05.2023');
    });

    it('should show correct date at startup (21.09.2020)', async () => {
        render(WalterDatePicker, {
            props: { value: '2020-09-21', labelText: 'Datum' }
        });

        const input = document.getElementsByTagName('input').item(0);

        expect(input).toBeDefined();
        expect(input).toHaveProperty('value');
        expect(input!.value).toBe('21.09.2020');
    });
});
