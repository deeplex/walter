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
import {
    baueGarageMatrix,
    baueMietMatrix,
    baueZaehlerMatrix,
    zaehlerDetail,
    baueRechnungsMatrix,
    filtereMatrix,
    letzterStandText,
    mietDetail,
    rechnungsDetail,
    zaehleOffen,
    zaehleOffeneInGruppe
} from './matrix';

const umlagen = [
    {
        id: 1,
        typ: { id: 1, text: 'Wasser' },
        selectedWohnungen: [
            { id: 10, text: 'Haus 1 – EG' },
            { id: 11, text: 'Haus 1 – OG' }
        ],
        wohnungenKurz: [
            { id: 10, adresse: 'Haus 1', bezeichnung: 'EG' },
            { id: 11, adresse: 'Haus 1', bezeichnung: 'OG' }
        ],
        betriebskostenrechnungsJahre: [2025]
    },
    {
        id: 2,
        typ: { id: 2, text: 'Müll' },
        selectedWohnungen: [{ id: 10, text: 'Haus 1 – EG' }],
        betriebskostenrechnungsJahre: []
    }
];

describe('baueRechnungsMatrix', () => {
    it('färbt alle Wohnungen einer Umlage gemeinsam', () => {
        const m = baueRechnungsMatrix(umlagen, 2025);
        const eg = m.zeilen.find((z) => z.id === '10')!;
        const og = m.zeilen.find((z) => z.id === '11')!;

        expect(eg.zellen['1']?.status).toBe('erledigt');
        expect(og.zellen['1']?.status).toBe('erledigt');
        expect(eg.zellen['2']?.status).toBe('offen');
    });

    it('lässt Zellen weg, die für die Wohnung nicht gelten', () => {
        const m = baueRechnungsMatrix(umlagen, 2025);
        const og = m.zeilen.find((z) => z.id === '11')!;
        expect(og.zellen['2']).toBeUndefined();
    });

    it('ignoriert beendete Umlagen und solche ohne Wohnungen', () => {
        const m = baueRechnungsMatrix(
            [
                { ...umlagen[0], ende: '2023-12-31' },
                { ...umlagen[1], selectedWohnungen: [] }
            ],
            2025
        );
        expect(m.spalten).toEqual([]);
        expect(m.zeilen).toEqual([]);
    });
});

describe('zaehleOffen / Nur Offene', () => {
    it('zählt nur offene Zellen', () => {
        expect(zaehleOffen(baueRechnungsMatrix(umlagen, 2025))).toBe(1);
        expect(zaehleOffen(baueRechnungsMatrix(umlagen, 2024))).toBe(3);
    });

    it('blendet vollständige Zeilen mit „Nur Offene" aus', () => {
        const m = baueRechnungsMatrix(umlagen, 2025);
        expect(filtereMatrix(m, true, '').zeilen.map((z) => z.id)).toEqual([
            '10'
        ]);
        expect(filtereMatrix(m, false, '').zeilen).toHaveLength(2);
    });

    it('filtert nach Suchtext', () => {
        const m = baueRechnungsMatrix(umlagen, 2024);
        expect(filtereMatrix(m, false, 'og').zeilen.map((z) => z.id)).toEqual([
            '11'
        ]);
    });
});

describe('baueMietMatrix', () => {
    const daten = [
        {
            vertragId: 7,
            vertragBezeichnung: 'Haus – EG | Müller',
            adresse: 'Haus',
            wohnung: 'EG',
            mieter: 'Müller',
            monate: [
                { monat: '2026-01-01', soll: 500, gezahlt: 500, offen: 0 },
                { monat: '2026-02-01', soll: 500, gezahlt: 200, offen: 300 },
                { monat: '2026-03-01', soll: 500, gezahlt: 0, offen: 500 }
            ]
        }
    ];

    it('legt zwölf Monatsspalten an', () => {
        expect(baueMietMatrix(daten).spalten).toHaveLength(12);
    });

    it('bestimmt den Status je Monat und lässt künftige Monate leer', () => {
        const [zeile] = baueMietMatrix(daten).zeilen;
        expect(zeile.zellen['1']?.status).toBe('erledigt');
        expect(zeile.zellen['2']?.status).toBe('teilweise');
        expect(zeile.zellen['3']?.status).toBe('offen');
        expect(zeile.zellen['4']).toBeUndefined();
        expect(zeile.zellen['2']?.ref).toBe('2026-02-01');
    });

    it('wertet eine Überzahlung als erledigt', () => {
        const [zeile] = baueMietMatrix([
            {
                ...daten[0],
                monate: [
                    {
                        monat: '2026-01-01',
                        soll: 500,
                        gezahlt: 600,
                        offen: -100
                    }
                ]
            }
        ]).zeilen;
        expect(zeile.zellen['1']?.status).toBe('erledigt');
    });
});

describe('mietDetail', () => {
    const monat = { monat: '2026-02-01', soll: 650, gezahlt: 500, offen: 150 };
    const konto = {
        monate: [
            {
                jahr: 2026,
                monat: 2,
                zahlungen: [
                    {
                        datum: '2026-02-03',
                        betrag: 500,
                        zahler: 'Müller',
                        art: 'Kaltmiete'
                    },
                    {
                        datum: '2026-02-03',
                        betrag: 20,
                        art: 'Garage',
                        garage: 'G1'
                    }
                ]
            },
            { jahr: 2026, monat: 3, zahlungen: [] }
        ]
    };

    it('zeigt die Zahlungen des Monats mit Art und Zahler', () => {
        const d = mietDetail(monat, konto, '/vertraege/1#mietkonto');
        expect(d.eintraege).toHaveLength(2);
        expect(d.eintraege[0].text).toBe('Kaltmiete von Müller');
        expect(d.eintraege[1].text).toBe('Garage G1');
        expect(d.link?.href).toBe('/vertraege/1#mietkonto');
    });

    it('liefert ohne Mietkonto eine leere Liste', () => {
        expect(mietDetail(monat, undefined, undefined).eintraege).toEqual([]);
    });
});

describe('rechnungsDetail', () => {
    const rechnungen = [
        {
            id: 'a',
            datum: '2026-03-01',
            betrag: 100,
            betreffendesJahr: 2026,
            isBezahlt: true,
            umlage: { id: 5 }
        },
        {
            id: 'b',
            datum: '2025-03-01',
            betrag: 90,
            betreffendesJahr: 2025,
            isBezahlt: true,
            umlage: { id: 5 }
        },
        {
            id: 'c',
            datum: '2026-04-01',
            betrag: 10,
            betreffendesJahr: 2026,
            isBezahlt: false,
            umlage: { id: 6 }
        }
    ];

    it('filtert auf Umlage und Jahr', () => {
        const d = rechnungsDetail(rechnungen, '5', 2026, undefined);
        expect(d.eintraege.map((e) => e.href)).toEqual(['/buchungssaetze/a']);
        expect(d.zusammenfassung[1].wert).toContain('100');
    });
});

describe('Gruppierung', () => {
    it('gruppiert die Rechnungsmatrix nach Adresse mit getrennter Wohnung', () => {
        const m = baueRechnungsMatrix(umlagen, 2025);
        const eg = m.zeilen.find((z) => z.id === '10')!;
        expect(eg.gruppe).toBe('Haus 1');
        expect(eg.info).toEqual(['EG']);
        expect(m.infoSpalten).toEqual(['Wohnung']);
    });

    it('gruppiert die Mietmatrix nach Adresse und trennt Wohnung und Mieter', () => {
        const m = baueMietMatrix([
            {
                vertragId: 2,
                vertragBezeichnung: 'B – 2 | Z',
                adresse: 'B',
                wohnung: '2',
                mieter: 'Z',
                monate: []
            },
            {
                vertragId: 1,
                vertragBezeichnung: 'A – 1 | Y',
                adresse: 'A',
                wohnung: '1',
                mieter: 'Y',
                monate: []
            }
        ]);
        expect(m.infoSpalten).toEqual(['Wohnung', 'Mieter']);
        expect(m.zeilen.map((z) => z.gruppe)).toEqual(['A', 'B']);
        expect(m.zeilen[0].info).toEqual(['1', 'Y']);
        expect(zaehleOffeneInGruppe(m, 'A')).toBe(0);
    });
});

describe('baueGarageMatrix', () => {
    it('gruppiert Garagen nach Adresse und färbt die Monate', () => {
        const m = baueGarageMatrix([
            {
                garageVertragId: 3,
                bezeichnung: 'G1 | Müller',
                adresse: 'Hof 1',
                garage: 'G1',
                mieter: 'Müller',
                monate: [
                    { monat: '2026-01-01', soll: 50, gezahlt: 50, offen: 0 },
                    { monat: '2026-02-01', soll: 50, gezahlt: 0, offen: 50 }
                ]
            }
        ]);
        expect(m.infoSpalten).toEqual(['Garage', 'Mieter']);
        expect(m.zeilen[0].gruppe).toBe('Hof 1');
        expect(m.zeilen[0].zellen['1']?.status).toBe('erledigt');
        expect(m.zeilen[0].zellen['2']?.status).toBe('offen');
    });
});

describe('baueZaehlerMatrix', () => {
    const daten = [
        {
            id: 1,
            kennnummer: 'Z1',
            typText: 'Strom',
            adresse: 'Hof 1',
            wohnung: 'EG',
            lastStand: 100,
            lastEinheit: 'kWh',
            staende: [
                { datum: '2026-05-01', stand: 120, einheit: 'kWh' },
                { datum: '2026-11-01', stand: 150, einheit: 'kWh' }
            ]
        },
        {
            id: 2,
            kennnummer: 'Z2',
            typText: 'Wasser',
            adresse: 'Hof 1',
            wohnung: '',
            staende: []
        }
    ];

    it('hat eine Spalte; erledigt ist ein Zähler mit Stand im Jahr', () => {
        const m = baueZaehlerMatrix(daten, 2026);
        expect(m.spalten).toHaveLength(1);
        expect(
            m.zeilen.find((z) => z.id === '1')?.zellen['stand']?.status
        ).toBe('erledigt');
        expect(
            m.zeilen.find((z) => z.id === '2')?.zellen['stand']?.status
        ).toBe('offen');
    });

    it('zeigt bei mehreren Ständen die Anzahl und nennt Allgemeinzähler so', () => {
        const m = baueZaehlerMatrix(daten, 2026);
        expect(m.zeilen.find((z) => z.id === '1')?.zellen['stand']?.text).toBe(
            '2'
        );
        expect(m.zeilen.find((z) => z.id === '2')?.info?.[0]).toBe('Allgemein');
    });

    it('listet die Stände des Jahres mit Einheit im Dialog', () => {
        const d = zaehlerDetail(daten[0], '/zaehler/1');
        expect(d.eintraege.map((e) => e.wert)).toEqual(['120 kWh', '150 kWh']);
        expect(d.wertLabel).toBe('Stand');
    });
});

describe('filtereMatrix', () => {
    const m = baueRechnungsMatrix(umlagen, 2024);

    it('findet Zeilen über ihren Text und behält alle Spalten', () => {
        const a = filtereMatrix(m, false, 'og');
        expect(a.zeilen.map((z) => z.id)).toEqual(['11']);
        expect(a.spalten).toHaveLength(m.spalten.length);
    });

    it('findet Kostenarten über die Spalte und zeigt nur diese Spalte', () => {
        const a = filtereMatrix(m, false, 'müll');
        expect(a.spalten.map((s) => s.label)).toEqual(['Müll']);
        // Nur die Wohnung, für die der Typ „Müll“ gilt, bleibt übrig.
        expect(a.zeilen.map((z) => z.id)).toEqual(['10']);
    });

    it('liefert ohne Treffer nichts', () => {
        expect(filtereMatrix(m, false, 'gibtesnicht').zeilen).toEqual([]);
    });

    it('wendet „Nur Offene“ auf die sichtbaren Spalten an', () => {
        // 2025: Wasser ist erledigt, Müll offen. Sucht man „Wasser“, ist nichts offen.
        const m25 = baueRechnungsMatrix(umlagen, 2025);
        expect(filtereMatrix(m25, true, 'wasser').zeilen).toEqual([]);
        expect(
            filtereMatrix(m25, true, 'müll').zeilen.map((z) => z.id)
        ).toEqual(['10']);
    });
});

describe('Suche mit mehreren Begriffen', () => {
    const m = baueRechnungsMatrix(umlagen, 2024);

    it('verlangt alle Begriffe, getrennt mit ;', () => {
        expect(
            filtereMatrix(m, false, 'haus 1; og').zeilen.map((z) => z.id)
        ).toEqual(['11']);
        expect(filtereMatrix(m, false, 'og; eg').zeilen).toEqual([]);
    });

    it('kombiniert einen Zeilenbegriff mit einer Spalte', () => {
        const a = filtereMatrix(m, false, 'eg; müll');
        expect(a.spalten.map((s) => s.label)).toEqual(['Müll']);
        expect(a.zeilen.map((z) => z.id)).toEqual(['10']);
        // Die OG-Wohnung hat keinen Müll-Typ und fällt heraus.
        expect(filtereMatrix(m, false, 'og; müll').zeilen).toEqual([]);
    });

    it('ignoriert leere Begriffe', () => {
        expect(filtereMatrix(m, false, 'og;;').zeilen.map((z) => z.id)).toEqual(
            ['11']
        );
    });
});

describe('letzterStandText', () => {
    it('zeigt Stand, Einheit und Datum', () => {
        const t = letzterStandText({
            lastStand: 1234.5,
            lastEinheit: 'kWh',
            lastDatum: '2025-05-01'
        });
        expect(t).toContain('1.234,5 kWh');
        expect(t).toContain('01.05.2025');
    });

    it('zeigt einen Strich ohne Stand', () => {
        expect(letzterStandText({})).toBe('–');
    });
});

describe('Felder ohne nötige Abrechnung', () => {
    it('stellt Wohnungen ohne nötige Abrechnung als ignoriert dar, auch mit Rechnung', () => {
        // Nur Wohnung 11 braucht eine Abrechnung; Wohnung 10 hat eine Rechnung, ist aber ignoriert.
        const m = baueRechnungsMatrix(umlagen, 2025, new Map([[11, false]]));
        expect(m.zeilen.find((z) => z.id === '10')?.zellen['1']?.status).toBe(
            'ignoriert'
        );
        expect(m.zeilen.find((z) => z.id === '10')?.zellen['2']?.status).toBe(
            'ignoriert'
        );
        expect(m.zeilen.find((z) => z.id === '11')?.zellen['1']?.status).toBe(
            'erledigt'
        );
    });

    it('zählt ignorierte Felder weder als offen noch als erledigt', () => {
        const m = baueRechnungsMatrix(
            umlagen,
            2024,
            new Map<number, boolean>()
        );
        expect(zaehleOffen(m)).toBe(0);
        expect(filtereMatrix(m, true, '').zeilen).toEqual([]);
    });

    it('lässt ohne Angabe alles gelten', () => {
        const m = baueRechnungsMatrix(umlagen, 2024);
        expect(zaehleOffen(m)).toBe(3);
    });

    it('zeigt bei mehreren Umlagen eines Typs das dringendste Feld', () => {
        const doppelt = [
            {
                ...umlagen[0],
                id: 7,
                betriebskostenrechnungsJahre: [2024],
                selectedWohnungen: [{ id: 10, text: 'Haus 1 – EG' }]
            },
            {
                ...umlagen[0],
                id: 8,
                betriebskostenrechnungsJahre: [],
                selectedWohnungen: [{ id: 10, text: 'Haus 1 – EG' }]
            }
        ];
        const m = baueRechnungsMatrix(doppelt, 2024);
        expect(m.zeilen[0].zellen['1']?.status).toBe('offen');
        expect(m.zeilen[0].zellen['1']?.ref).toBe('8');
    });

    it('stellt Zähler einer Wohnung ohne nötige Abrechnung als ignoriert dar', () => {
        const daten = [
            {
                id: 1,
                kennnummer: 'Z1',
                typText: 'Strom',
                adresse: 'A',
                wohnung: 'EG',
                wohnungId: 10,
                staende: []
            },
            {
                id: 2,
                kennnummer: 'Z2',
                typText: 'Strom',
                adresse: 'A',
                wohnung: 'OG',
                wohnungId: 11,
                staende: []
            },
            {
                id: 3,
                kennnummer: 'Z3',
                typText: 'Wasser',
                adresse: 'A',
                wohnung: '',
                wohnungId: null,
                staende: []
            }
        ];
        const m = baueZaehlerMatrix(daten, 2025, new Map([[11, false]]));
        const st = (id: string) =>
            m.zeilen.find((z) => z.id === id)?.zellen['stand']?.status;
        expect(st('1')).toBe('ignoriert');
        expect(st('2')).toBe('offen');
        // Allgemeine Zähler ohne Wohnung bleiben nötig.
        expect(st('3')).toBe('offen');
    });
});

describe('Abgeschlossene Abrechnung', () => {
    it('zählt offene Rechnungen als erledigt, wenn die Abrechnung der Wohnung gesendet ist', () => {
        // Wohnung 10: Abrechnung gesendet. Der Typ „Müll“ hat keine Rechnung, ist aber erledigt.
        const m = baueRechnungsMatrix(
            umlagen,
            2024,
            new Map([
                [10, true],
                [11, false]
            ])
        );
        expect(m.zeilen.find((z) => z.id === '10')?.zellen['2']?.status).toBe(
            'erledigt'
        );
        expect(
            m.zeilen.find((z) => z.id === '10')?.zellen['2']?.titel
        ).toContain('gesendet');
        // Wohnung 11: nicht gesendet, die Rechnung fehlt weiter.
        expect(m.zeilen.find((z) => z.id === '11')?.zellen['1']?.status).toBe(
            'offen'
        );
        expect(zaehleOffen(m)).toBe(1);
    });

    it('zählt Zähler ohne Stand als erledigt, wenn die Abrechnung gesendet ist', () => {
        const daten = [
            {
                id: 1,
                kennnummer: 'Z1',
                typText: 'Strom',
                adresse: 'A',
                wohnung: 'EG',
                wohnungId: 10,
                staende: []
            },
            {
                id: 2,
                kennnummer: 'Z2',
                typText: 'Strom',
                adresse: 'A',
                wohnung: 'OG',
                wohnungId: 11,
                staende: []
            }
        ];
        const m = baueZaehlerMatrix(
            daten,
            2025,
            new Map([
                [10, true],
                [11, false]
            ])
        );
        const st = (id: string) =>
            m.zeilen.find((z) => z.id === id)?.zellen['stand']?.status;
        expect(st('1')).toBe('erledigt');
        expect(st('2')).toBe('offen');
    });
});
