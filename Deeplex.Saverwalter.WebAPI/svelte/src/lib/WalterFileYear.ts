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

import type { WalterFile } from './WalterFile';

export type WalterFileYearGroup = { year: number; files: WalterFile[] };

/**
 * Die Dateien je Jahr, das neueste Jahr zuerst; innerhalb eines Jahres bleibt die Reihenfolge.
 * Dateien ohne Jahr (`year` 0) bilden die erste Gruppe, ohne Überschrift in der Oberfläche.
 */
export function groupFilesByYear(files: WalterFile[]): WalterFileYearGroup[] {
    const groups = new Map<number, WalterFile[]>();
    for (const file of files) {
        const year = file.year || 0;
        groups.set(year, [...(groups.get(year) ?? []), file]);
    }
    return [...groups.entries()]
        .map(([year, grouped]) => ({ year, files: grouped }))
        .sort((a, b) => b.year - a.year)
        .sort((a, b) => (a.year === 0 ? -1 : b.year === 0 ? 1 : 0));
}

/** Das Jahr einer Datei ohne Jahr (wie `FileLayout.NoYear` im Backend). */
export const noYear = 0;

/** Das Jahr als Text; 0 ist „Ohne Jahr“. */
export function yearLabel(year: number): string {
    return year > 0 ? `${year}` : 'Ohne Jahr';
}

/** Die Jahre zur Auswahl beim Hochladen und Wechseln: vom nächsten Jahr zurück. */
export function selectableYears(currentYear: number, span = 15): number[] {
    return Array.from({ length: span + 1 }, (_, i) => currentYear + 1 - i);
}
