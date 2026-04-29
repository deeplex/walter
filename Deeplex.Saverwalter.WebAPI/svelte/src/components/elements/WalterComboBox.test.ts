import { describe, expect, it } from 'vitest';

import { shouldFilterItem } from './WalterComboBox';

describe('WalterComboBox helpers', () => {
    it('returns true for empty filter values', () => {
        expect(shouldFilterItem({ id: 1, text: 'Berlin' } as never, '')).toBe(
            true
        );
    });

    it('matches all semicolon-separated tokens case-insensitively', () => {
        expect(
            shouldFilterItem(
                { id: 1, text: 'Wohnung Berlin Mitte' } as never,
                'berlin; mitte'
            )
        ).toBe(true);

        expect(
            shouldFilterItem(
                { id: 1, text: 'Wohnung Berlin Mitte' } as never,
                'berlin; hamburg'
            )
        ).toBe(false);
    });
});
