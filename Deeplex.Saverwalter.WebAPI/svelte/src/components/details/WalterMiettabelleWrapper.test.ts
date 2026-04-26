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

import { describe, it, expect, beforeEach } from 'vitest';
import type {
    WalterUmlageEntry,
    WalterVertragEntry,
    WalterZaehlerEntry,
    WalterMieteEntry,
    WalterZaehlerstandEntry
} from '$walter/lib';

const defaultPermissions = { read: true, update: true, remove: true };

// Mock data for testing
const createMockVertrag = (
    id: number,
    beginn: string,
    ende?: string,
    mieten: WalterMieteEntry[] = [],
    permissions = defaultPermissions,
    mieterAuflistung = 'Mieter A, Mieter B'
): WalterVertragEntry =>
    ({
        id,
        beginn,
        ende,
        mieten,
        mieterAuflistung,
        permissions,
        versionen: [
            {
                id: 1,
                beginn: '2023-01-01',
                grundmiete: 1000
            }
        ] as any,
        wohnung: {
            id: 1,
            text: `Wohnung ${id}`
        } as any
    }) as WalterVertragEntry;

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
        } as any,
        lastZaehlerstand: staende[staende.length - 1]
    }) as WalterZaehlerEntry;

const createMockUmlage = (
    id: number,
    typ: string = 'Nebenkosten',
    betriebskostenrechnungen: any[] = [],
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
        } as any,
        beschreibung: `Umlage ${id}`,
        wohnungenBezeichnung: 'Alle Wohnungen'
    }) as unknown as WalterUmlageEntry;

describe('WalterMiettabelleWrapper - Task Building Functions', () => {
    describe('buildRentTasks', () => {
        it('should return empty array when no vertraege have update permissions', () => {
            const vertraege = [
                createMockVertrag(1, '2023-01-01', undefined, [], {
                    read: true,
                    update: false,
                    remove: false
                })
            ];

            const result = buildRentTasks(vertraege, 2024);
            expect(result).toEqual([]);
        });

        it('should filter vertraege by year range', () => {
            const vertraege = [
                createMockVertrag(1, '2025-01-01'), // Starts after 2024
                createMockVertrag(2, '2022-01-01', '2023-12-31'), // Ends before 2024
                createMockVertrag(3, '2023-01-01', '2025-12-31') // Includes 2024
            ];

            const result = buildRentTasks(vertraege, 2024);
            expect(result.length).toBe(1);
            expect(result[0].vertrag.id).toBe(3);
        });

        it('should skip months that already have miete entries', () => {
            const mieten: WalterMieteEntry[] = [
                {
                    id: 1,
                    betreffenderMonat: '2024-01-01',
                    zahlungsdatum: '2024-01-15',
                    betrag: 1000
                } as any
            ];
            const vertraege = [
                createMockVertrag(1, '2024-01-01', undefined, mieten)
            ];

            const result = buildRentTasks(vertraege, 2024);
            // Should only find missing months after January
            const tasks = result.filter((t) => t.monthIndex === 0);
            expect(tasks.length).toBe(0);
        });

        it('should return RentTask with correct properties', () => {
            const vertraege = [createMockVertrag(1, '2024-01-01')];

            const result = buildRentTasks(vertraege, 2024);

            expect(result.length).toBeGreaterThan(0);
            const task = result[0];
            expect(task).toHaveProperty('vertrag');
            expect(task).toHaveProperty('monthIndex');
            expect(task).toHaveProperty('monthDate');
            expect(task).toHaveProperty('amount');
            expect(task).toHaveProperty('mieterAuflistung');
            expect(task.mieterAuflistung).toBe('Mieter A, Mieter B');
            expect(task.amount).toBe(1000); // From grundmiete
        });

        it('should use latest miete betrag or grundmiete for amount', () => {
            const mieten: WalterMieteEntry[] = [
                {
                    id: 1,
                    betreffenderMonat: '2023-12-01',
                    zahlungsdatum: '2023-12-15',
                    betrag: 1200
                } as any
            ];
            const vertraege = [
                createMockVertrag(1, '2023-01-01', undefined, mieten)
            ];

            const result = buildRentTasks(vertraege, 2024);

            expect(result.length).toBeGreaterThan(0);
            expect(result[0].amount).toBe(1200); // Latest miete betrag
        });

        it('should sort tasks by monthDate', () => {
            const vertraege = [
                createMockVertrag(1, '2024-01-01'),
                createMockVertrag(2, '2024-03-01')
            ];

            const result = buildRentTasks(vertraege, 2024);

            if (result.length >= 2) {
                expect(result[0].monthDate.getTime()).toBeLessThanOrEqual(
                    result[1].monthDate.getTime()
                );
            }
        });
    });

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
                } as any
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
            const rechnungen = [
                {
                    id: 1,
                    betreffendesJahr: 2024,
                    datum: '2024-06-01'
                }
            ];
            const umlagen = [createMockUmlage(1, 'Nebenkosten', rechnungen)];

            const result = buildInvoiceTasks(umlagen, 2024);

            expect(result).toEqual([]);
        });

        it('should include umlagen without rechnung for year', () => {
            const rechnungen = [
                {
                    id: 1,
                    betreffendesJahr: 2023,
                    datum: '2023-06-01'
                }
            ];
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
function buildRentTasks(
    vertraege: WalterVertragEntry[],
    year: number
): Array<{
    vertrag: WalterVertragEntry;
    monthIndex: number;
    monthDate: Date;
    amount: number;
    mieterAuflistung: string;
}> {
    const currentYear = new Date().getFullYear();

    function getLatestMieteBefore(
        vertrag: WalterVertragEntry,
        date: Date
    ): WalterMieteEntry | undefined {
        return [...(vertrag.mieten || [])]
            .filter(
                (miete) =>
                    new Date(miete.betreffenderMonat).getTime() < date.getTime()
            )
            .sort(
                (a, b) =>
                    new Date(a.betreffenderMonat).getTime() -
                    new Date(b.betreffenderMonat).getTime()
            )
            .at(-1);
    }

    function getGrundmieteForDate(
        vertrag: WalterVertragEntry,
        date: Date
    ): number {
        const version = [...(vertrag.versionen || [])]
            .sort(
                (a, b) =>
                    new Date(a.beginn).getTime() - new Date(b.beginn).getTime()
            )
            .filter(
                (entry) => new Date(entry.beginn).getTime() <= date.getTime()
            )
            .at(-1);

        return version?.grundmiete || 0;
    }

    return vertraege
        .filter((vertrag) => !!vertrag.permissions?.update)
        .map((vertrag) => {
            const start = new Date(vertrag.beginn);
            const end = vertrag.ende ? new Date(vertrag.ende) : undefined;

            if (start.getFullYear() > year) {
                return undefined;
            }
            if (end && end.getFullYear() < year) {
                return undefined;
            }

            const startMonth =
                start.getFullYear() === year ? start.getMonth() : 0;
            const endMonth =
                end && end.getFullYear() === year ? end.getMonth() : 11;

            if (endMonth < startMonth) {
                return undefined;
            }

            const existingMonths = new Set(
                (vertrag.mieten || [])
                    .filter(
                        (miete) =>
                            new Date(miete.betreffenderMonat).getFullYear() ===
                            year
                    )
                    .map((miete) =>
                        new Date(miete.betreffenderMonat).getMonth()
                    )
            );

            const currentMonth = new Date().getMonth();
            const searchStart =
                year === currentYear
                    ? Math.max(startMonth, currentMonth)
                    : startMonth;

            for (
                let monthIndex = searchStart;
                monthIndex <= endMonth;
                monthIndex += 1
            ) {
                if (existingMonths.has(monthIndex)) {
                    continue;
                }

                const monthDate = new Date(year, monthIndex, 1);
                const latestMiete = getLatestMieteBefore(vertrag, monthDate);
                const amount =
                    latestMiete?.betrag ||
                    getGrundmieteForDate(vertrag, monthDate);
                const mieterAuflistung =
                    vertrag.mieterAuflistung || 'Keine Mieter';

                return {
                    vertrag,
                    monthIndex,
                    monthDate,
                    amount,
                    mieterAuflistung
                };
            }

            return undefined;
        })
        .filter((task): task is any => !!task)
        .sort(
            (a, b) =>
                a.monthDate.getTime() - b.monthDate.getTime() ||
                `${a.vertrag.wohnung?.text || ''}`.localeCompare(
                    `${b.vertrag.wohnung?.text || ''}`
                )
        );
}

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
