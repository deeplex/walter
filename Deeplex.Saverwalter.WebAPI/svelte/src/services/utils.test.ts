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

import { get } from 'svelte/store';
import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('$app/navigation', () => ({
    goto: vi.fn()
}));

import { goto } from '$app/navigation';
import { changeTracker, walterModalControl } from '$walter/store';
import {
    convertDateCanadian,
    convertDateGerman,
    convertEuro,
    convertFixed2,
    convertM2,
    convertPercent,
    convertTime,
    walter_goto,
    walter_subscribe_reset_changeTracker,
    walter_update_value
} from '$walter/services/utils';

beforeEach(() => {
    vi.clearAllMocks();
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

describe('convertDateCanadian tests', () => {
    it('should be 2023-05-24', () => {
        const date = new Date('2023-05-24');

        const convertedDate = convertDateCanadian(date);

        expect(convertedDate).toBe('2023-05-24');
    });
});

describe('convertDateGerman tests', () => {
    it('should be 24.05.2023', () => {
        const date = new Date('2023-05-24');

        const convertedDate = convertDateGerman(date);

        expect(convertedDate).toBe('24.05.2023');
    });
});

describe('format helper tests', () => {
    it('should convert times, euro, percent, m2 and fixed2 values', () => {
        expect(convertTime('2023-05-24T10:11:12Z')).toBeDefined();
        expect(convertTime(undefined)).toBeUndefined();
        expect(convertEuro(12.3)).toBe('12.30 €');
        expect(convertEuro(undefined)).toBe('0.00 €');
        expect(convertPercent(0.1234)).toBe('12.34%');
        expect(convertPercent(undefined)).toBe('0.00%');
        expect(convertM2(42)).toBe('42.00 m²');
        expect(convertM2(undefined)).toBe('0.00 m²');
        expect(convertFixed2(5)).toBe('5.00');
        expect(convertFixed2(undefined)).toBe('0.00');
    });
});

describe('walter_goto tests', () => {
    it('should call goto directly when there are no unsaved changes', async () => {
        vi.mocked(goto).mockResolvedValue();

        await walter_goto('/wohnungen', { replaceState: true });

        expect(goto).toHaveBeenCalledWith('/wohnungen', {
            replaceState: true
        });
        expect(get(walterModalControl).open).toBe(false);
    });

    it('should open a confirmation modal when unsaved changes exist', async () => {
        vi.mocked(goto).mockResolvedValue();
        changeTracker.set(2);

        walter_goto('/wohnungen/1');

        const modal = get(walterModalControl);
        expect(modal.open).toBe(true);
        expect(modal.modalHeading).toBe('Seite verlassen?');

        await modal.submit();

        expect(get(changeTracker)).toBe(0);
        expect(goto).toHaveBeenCalledWith('/wohnungen/1', undefined);
    });
});

describe('changeTracker helper tests', () => {
    it('should call updateLastSavedValue when tracker resets through -1', () => {
        const updateLastSavedValue = vi.fn();

        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        changeTracker.set(-1);

        expect(updateLastSavedValue).toHaveBeenCalledTimes(1);
    });

    it('should keep the old value when nothing changed', () => {
        changeTracker.set(0);

        const result = walter_update_value('saved', 'same', 'same');

        expect(result).toBe('same');
        expect(get(changeTracker)).toBe(0);
    });

    it('should increment tracker when moving away from the last saved value', () => {
        changeTracker.set(0);

        const result = walter_update_value('saved', 'saved', 'edited');

        expect(result).toBe('edited');
        expect(get(changeTracker)).toBe(1);
    });

    it('should decrement tracker when returning to the last saved value', () => {
        changeTracker.set(1);

        const result = walter_update_value('saved', 'edited', 'saved');

        expect(result).toBe('saved');
        expect(get(changeTracker)).toBe(0);
    });

    it('should return the new value without changing the tracker otherwise', () => {
        changeTracker.set(4);

        const result = walter_update_value('saved', 'edited', 'other');

        expect(result).toBe('other');
        expect(get(changeTracker)).toBe(4);
    });
});
