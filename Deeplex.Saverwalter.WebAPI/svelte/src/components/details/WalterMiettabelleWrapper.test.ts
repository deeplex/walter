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
import type {
    WalterBetriebskostenrechnungEntry,
    WalterSelectionEntry,
    WalterUmlageEntry,
    WalterZaehlerEntry,
    WalterZaehlerstandEntry
} from '$walter/lib';

const defaultPermissions = { read: true, update: true, remove: true };

// Mock data for testing
const createMockZaehler = (
    id: number,
    kennnummer: string,
    staende: WalterZaehlerstandEntry[] = [],
    permissions = defaultPermissions
): WalterZaehlerEntry =>
    ({
        id,
        kennnummer,
        staende,
        permissions,
        typ: {
            id: 1,
            text: 'Strom'
        } as WalterSelectionEntry,
        lastZaehlerstand: staende[staende.length - 1]
    }) as WalterZaehlerEntry;

const createMockRechnung = (
    id: number,
    betreffendesJahr: number,
    datum: string
): WalterBetriebskostenrechnungEntry =>
    ({
        id,
        betreffendesJahr,
        datum
    }) as unknown as WalterBetriebskostenrechnungEntry;

const createMockUmlage = (
    id: number,
    typ: string = 'Nebenkosten',
    betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[] = [],
    permissions = defaultPermissions
): WalterUmlageEntry =>
    ({
        id,
        betriebskostenrechnungen,
        permissions,
        zaehler: [],
        typ: {
            id: 1,
            text: typ
        } as WalterSelectionEntry,
        beschreibung: `Umlage ${id}`,
        wohnungenBezeichnung: 'Alle Wohnungen'
    }) as unknown as WalterUmlageEntry;

describe('WalterMiettabelleWrapper - Task Building Functions', () => {
    describe('buildMeterTasks', () => {
        it('should deduplicate zaehler by ID', () => {
            const zaehler1: WalterZaehlerEntry = createMockZaehler(1, 'Z001');
            const zaehler2: WalterZaehlerEntry = createMockZaehler(2, 'Z002');

            const umlagen = [
                createMockUmlage(1, 'Nebenkosten'),
                createMockUmlage(2, 'Wasser')
            ];
            umlagen[0].zaehler = [zaehler1, zaehler2];
            umlagen[1].zaehler = [zaehler1]; // Duplicate

            const result = buildMeterTasks(umlagen, 2024);

            const ids = result.map((t) => t.zaehler.id);
            expect(new Set(ids).size).toBe(ids.length); // All unique
        });

        it('should filter by update permissions', () => {
            const zaehler: WalterZaehlerEntry = createMockZaehler(
                1,
                'Z001',
                [],
                {
                    read: true,
                    update: false,
                    remove: false
                }
            );
            const umlagen = [createMockUmlage(1)];
            umlagen[0].zaehler = [zaehler];

            const result = buildMeterTasks(umlagen, 2024);

            expect(result).toEqual([]);
        });

        it('should exclude zaehler with past end dates', () => {
            const zaehler: WalterZaehlerEntry = createMockZaehler(1, 'Z001');
            zaehler.ende = '2023-12-31';

            const umlagen = [createMockUmlage(1)];
            umlagen[0].zaehler = [zaehler];

            const result = buildMeterTasks(umlagen, 2024);

            expect(result).toEqual([]);
        });

        it('should exclude zaehler with existing readings for year', () => {
            const staende: WalterZaehlerstandEntry[] = [
                {
                    id: 1,
                    datum: '2024-03-15',
                    stand: 12345,
                    einheit: 'kWh'
                } as WalterZaehlerstandEntry
            ];
            const zaehler: WalterZaehlerEntry = createMockZaehler(
                1,
                'Z001',
                staende
            );

            const umlagen = [createMockUmlage(1)];
            umlagen[0].zaehler = [zaehler];

            const result = buildMeterTasks(umlagen, 2024);

            expect(result).toEqual([]);
        });

        it('should return MeterTask with correct properties', () => {
            const zaehler: WalterZaehlerEntry = createMockZaehler(1, 'Z001');
            const umlagen = [createMockUmlage(1)];
            umlagen[0].zaehler = [zaehler];

            const result = buildMeterTasks(umlagen, 2024);

            expect(result.length).toBe(1);
            expect(result[0]).toHaveProperty('zaehler');
            expect(result[0].zaehler.kennnummer).toBe('Z001');
        });

        it('should sort by kennnummer', () => {
            const zaehler1: WalterZaehlerEntry = createMockZaehler(1, 'Z002');
            const zaehler2: WalterZaehlerEntry = createMockZaehler(2, 'Z001');

            const umlagen = [createMockUmlage(1)];
            umlagen[0].zaehler = [zaehler1, zaehler2];

            const result = buildMeterTasks(umlagen, 2024);

            expect(result[0].zaehler.kennnummer).toBe('Z001');
            expect(result[1].zaehler.kennnummer).toBe('Z002');
        });
    });

    describe('buildInvoiceTasks', () => {
        it('should filter by update permissions', () => {
            const umlagen = [
                createMockUmlage(1, 'Nebenkosten', [], {
                    read: true,
                    update: false,
                    remove: false
                })
            ];

            const result = buildInvoiceTasks(umlagen, 2024);

            expect(result).toEqual([]);
        });

        it('should exclude umlagen with existing rechnung for year', () => {
            const rechnungen = [createMockRechnung(1, 2024, '2024-06-01')];
            const umlagen = [createMockUmlage(1, 'Nebenkosten', rechnungen)];

            const result = buildInvoiceTasks(umlagen, 2024);

            expect(result).toEqual([]);
        });

        it('should include umlagen without rechnung for year', () => {
            const rechnungen = [createMockRechnung(1, 2023, '2023-06-01')];
            const umlagen = [createMockUmlage(1, 'Nebenkosten', rechnungen)];

            const result = buildInvoiceTasks(umlagen, 2024);

            expect(result.length).toBe(1);
            expect(result[0].umlage.id).toBe(1);
        });

        it('should return InvoiceTask with correct properties', () => {
            const umlagen = [createMockUmlage(1, 'Nebenkosten')];

            const result = buildInvoiceTasks(umlagen, 2024);

            expect(result.length).toBe(1);
            expect(result[0]).toHaveProperty('umlage');
            expect(result[0].umlage.typ.text).toBe('Nebenkosten');
        });

        it('should sort by typ and description', () => {
            const umlagen = [
                createMockUmlage(1, 'Wasser'),
                createMockUmlage(2, 'Nebenkosten'),
                createMockUmlage(3, 'Nebenkosten')
            ];
            umlagen[2].beschreibung = 'Nebenkosten B';

            const result = buildInvoiceTasks(umlagen, 2024);

            // Verify sorting by comparing adjacent elements
            for (let i = 0; i < result.length - 1; i++) {
                const textA = `${result[i].umlage.typ?.text || ''} ${result[i].umlage.wohnungenBezeichnung || result[i].umlage.beschreibung || ''}`;
                const textB = `${result[i + 1].umlage.typ?.text || ''} ${result[i + 1].umlage.wohnungenBezeichnung || result[i + 1].umlage.beschreibung || ''}`;
                expect(textA.localeCompare(textB)).toBeLessThanOrEqual(0);
            }
        });
    });
});

// Helper functions that mirror the component logic
function buildMeterTasks(
    umlagen: WalterUmlageEntry[],
    year: number
): Array<{ zaehler: WalterZaehlerEntry }> {
    const byId = new Map<number, WalterZaehlerEntry>();
    umlagen.forEach((umlage) => {
        (umlage.zaehler || []).forEach((zaehler) => {
            if (!byId.has(zaehler.id)) {
                byId.set(zaehler.id, zaehler);
            }
        });
    });

    function hasMeterReadingForYear(zaehler: WalterZaehlerEntry, year: number) {
        const hasInList = (zaehler.staende || []).some(
            (stand) => new Date(stand.datum).getFullYear() === year
        );
        const lastYear = zaehler.lastZaehlerstand
            ? new Date(zaehler.lastZaehlerstand.datum).getFullYear()
            : undefined;

        return hasInList || lastYear === year;
    }

    return [...byId.values()]
        .filter((zaehler) => !!zaehler.permissions?.update)
        .filter((zaehler) => {
            if (!zaehler.ende) {
                return true;
            }

            return new Date(zaehler.ende).getFullYear() >= year;
        })
        .filter((zaehler) => !hasMeterReadingForYear(zaehler, year))
        .sort((a, b) => `${a.kennnummer}`.localeCompare(`${b.kennnummer}`))
        .map((zaehler) => ({ zaehler }));
}

function buildInvoiceTasks(
    umlagen: WalterUmlageEntry[],
    year: number
): Array<{ umlage: WalterUmlageEntry }> {
    return umlagen
        .filter((umlage) => !!umlage.permissions?.update)
        .filter(
            (umlage) =>
                !(umlage.betriebskostenrechnungen || []).some(
                    (rechnung) => rechnung.betreffendesJahr === year
                )
        )
        .sort((a, b) => {
            const textA = `${a.typ?.text || ''} ${a.wohnungenBezeichnung || a.beschreibung || ''}`;
            const textB = `${b.typ?.text || ''} ${b.wohnungenBezeichnung || b.beschreibung || ''}`;

            return textA.localeCompare(textB);
        })
        .map((umlage) => ({ umlage }));
}
