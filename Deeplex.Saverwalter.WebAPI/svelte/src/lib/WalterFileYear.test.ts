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

import { describe, expect, it } from 'vitest';
import { WalterFile } from './WalterFile';
import { groupFilesByYear, selectableYears, yearLabel } from './WalterFileYear';

const file = (fileName: string, year?: number) =>
    new WalterFile(
        fileName,
        `/api/x/1/files/${fileName}`,
        0,
        1,
        undefined,
        undefined,
        year
    );

describe('groupFilesByYear', () => {
    it('gruppiert nach Jahr, das neueste zuerst, und behält die Reihenfolge', () => {
        const groups = groupFilesByYear([
            file('a', 2022),
            file('b', 2024),
            file('c', 2022),
            file('d', 2023)
        ]);

        expect(groups.map((g) => g.year)).toEqual([2024, 2023, 2022]);
        expect(groups[2].files.map((f) => f.fileName)).toEqual(['a', 'c']);
    });

    it('legt Dateien ohne Jahr in eine eigene Gruppe am Anfang', () => {
        const groups = groupFilesByYear([
            file('a', 2023),
            file('ohne'),
            file('b', 2024)
        ]);

        expect(groups.map((g) => g.year)).toEqual([0, 2024, 2023]);
        expect(groups[0].files.map((f) => f.fileName)).toEqual(['ohne']);
    });

    it('ist ohne Dateien leer', () => {
        expect(groupFilesByYear([])).toEqual([]);
    });
});

describe('selectableYears', () => {
    it('reicht vom nächsten Jahr zurück', () => {
        const years = selectableYears(2026, 3);

        expect(years).toEqual([2027, 2026, 2025, 2024]);
    });
});

describe('yearLabel', () => {
    it('nennt 0 „Ohne Jahr“', () => {
        expect(yearLabel(2024)).toBe('2024');
        expect(yearLabel(0)).toBe('Ohne Jahr');
    });
});
