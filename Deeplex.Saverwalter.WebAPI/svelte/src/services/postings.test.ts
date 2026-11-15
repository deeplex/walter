// Copyright (C) 2023-2026  Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

import { describe, it, expect } from 'vitest';
import { gruppiereNachId, zeitraumText } from './postings';

describe('gruppiereNachId', () => {
    it('fasst Monate pro Vertrag zusammen und summiert', () => {
        const result = gruppiereNachId([
            { id: 1, bezeichnung: 'A', monat: '2026-03-01', betrag: 100 },
            { id: 1, bezeichnung: 'A', monat: '2026-01-01', betrag: 50 },
            { id: 2, bezeichnung: 'B', monat: '2026-02-01', betrag: 10 }
        ]);

        expect(result).toHaveLength(2);
        expect(result[0]).toMatchObject({
            id: 1,
            anzahl: 2,
            summe: 150,
            aeltesterMonat: '2026-01-01',
            neuesterMonat: '2026-03-01'
        });
    });

    it('sortiert den am längsten offenen Eintrag nach oben', () => {
        const result = gruppiereNachId([
            { id: 2, bezeichnung: 'Neu', monat: '2026-05-01', betrag: 1 },
            { id: 1, bezeichnung: 'Alt', monat: '2025-11-01', betrag: 1 }
        ]);
        expect(result.map((g) => g.id)).toEqual([1, 2]);
    });

    it('liefert bei leerer Eingabe eine leere Liste', () => {
        expect(gruppiereNachId([])).toEqual([]);
    });
});

describe('zeitraumText', () => {
    it('zeigt bei einem Monat nur diesen', () => {
        const [g] = gruppiereNachId([
            { id: 1, bezeichnung: 'A', monat: '2026-03-01', betrag: 1 }
        ]);
        expect(zeitraumText(g)).not.toContain('–');
    });

    it('zeigt bei mehreren Monaten einen Bereich', () => {
        const [g] = gruppiereNachId([
            { id: 1, bezeichnung: 'A', monat: '2026-01-01', betrag: 1 },
            { id: 1, bezeichnung: 'A', monat: '2026-04-01', betrag: 1 }
        ]);
        expect(zeitraumText(g)).toContain('–');
    });
});
