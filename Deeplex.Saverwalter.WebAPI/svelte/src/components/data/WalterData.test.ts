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

import { describe, expect, it } from 'vitest';

import {
    walter_data_aufwendungen,
    walter_data_mea,
    walter_data_ne,
    walter_data_nf,
    walter_data_rechnungen_year,
    walter_data_rechnungentabelle,
    walter_data_wf
} from './WalterData';

describe('WalterData helpers', () => {
    it('builds yearly rechnungen and aufwendungen datasets', () => {
        const yearly = walter_data_rechnungen_year('Jahre', 'Wasser', [
            { betreffendesJahr: 2024, betrag: 120 },
            { betreffendesJahr: 2025, betrag: 125 }
        ] as never);

        const aufwendungen = walter_data_aufwendungen('Aufwendungen', [
            {
                aussteller: { text: 'Firma A' },
                datum: '2026-01-10',
                betrag: 200
            }
        ] as never);

        expect(yearly.options.title).toBe('Jahre');
        expect(yearly.data[1]).toEqual({
            group: 'Wasser',
            key: '2025',
            value: 125
        });
        expect(aufwendungen.data).toEqual([
            {
                group: 'Firma A',
                date: '2026-01-10',
                value: 200
            }
        ]);
    });

    it('builds wohnung scalar datasets (wf/nf/mea/ne)', () => {
        const wohnungen = [
            {
                bezeichnung: 'W1',
                wohnflaeche: 75,
                nutzflaeche: 60,
                miteigentumsanteile: 120,
                einheiten: 1
            },
            {
                bezeichnung: 'W2',
                wohnflaeche: 55,
                nutzflaeche: 40,
                miteigentumsanteile: 95,
                einheiten: 1
            }
        ] as never;

        expect(walter_data_wf('WF', wohnungen).data[0].value).toBe(75);
        expect(walter_data_nf('NF', wohnungen).data[1].value).toBe(40);
        expect(walter_data_mea('MEA', wohnungen).data[0].value).toBe(120);
        expect(walter_data_ne('NE', wohnungen).data[1].value).toBe(1);
    });

    it('builds rechnungentabelle heatmap and normalizes non-positive values', () => {
        const config = walter_data_rechnungentabelle(
            [
                {
                    id: 1,
                    typ: { id: 11, text: 'Wasser' },
                    selectedWohnungen: [
                        { id: 101, text: 'W1' },
                        { id: 102, text: 'W2' }
                    ],
                    betriebskostenrechnungen: [
                        { betreffendesJahr: 2026, betrag: 50 },
                        { betreffendesJahr: 2025, betrag: 20 }
                    ]
                },
                {
                    id: 2,
                    typ: { id: 12, text: 'Hausstrom' },
                    selectedWohnungen: [{ id: 101, text: 'W1' }],
                    betriebskostenrechnungen: [
                        { betreffendesJahr: 2026, betrag: -10 }
                    ]
                }
            ] as never,
            2026
        );

        expect(config.data).toContainEqual(
            expect.objectContaining({
                group: 'W1',
                key: 'Wasser',
                value: 50,
                wohnungId: '101'
            })
        );
        expect(config.data).toContainEqual(
            expect.objectContaining({
                group: 'W1',
                key: 'Hausstrom',
                value: undefined
            })
        );
        expect(config.options.height).toBe('20em');
    });
});
