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

import type {
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
    WalterKontoMonatsSumme,
    WalterUmlageEntry,
    WalterWohnungEntry
} from '$walter/lib';
import type { WalterMietzahlungListEntry } from '$walter/lib/WalterMietzahlung';

const baseOptions = {
    legend: { enabled: false },
    height: '400px',
    tooltip: {
        truncation: {
            threshold: 999
        }
    }
} as WalterDataOptionsType;

export function walter_data_rechnungen_year(
    title: string,
    typ: string,
    rechnungen: WalterBetriebskostenrechnungEntry[]
): WalterDataConfigType {
    const options = {
        ...baseOptions,
        title,
        axes: {
            bottom: { mapsTo: 'key', scaleType: 'labels' },
            left: {
                mapsTo: 'value',
                scaleType: 'linear',
                includeZero: false
            }
        }
    };

    const data = rechnungen.map((rechnung) => ({
        group: `${typ}`,
        key: `${rechnung.betreffendesJahr}`,
        value: rechnung.betrag
    }));

    return { data, options };
}

export function walter_data_aufwendungen(
    title: string,
    aufwendungen: WalterErhaltungsaufwendungEntry[]
): WalterDataConfigType {
    const options = {
        ...baseOptions,
        title,
        axes: {
            left: { mapsTo: 'value', scaleType: 'log' },
            bottom: { mapsTo: 'date', scaleType: 'time' }
        },
        legend: { enabled: true }
    };

    const data = aufwendungen.map((aufwendung) => ({
        group: `${aufwendung.aussteller?.text ?? 'Unbekannt'}`,
        date: aufwendung.datum,
        value: aufwendung.betrag
    }));

    return { data, options };
}

export function walter_data_wf(
    title: string,
    wohnungen: WalterWohnungEntry[]
): WalterDataConfigType {
    const options = { ...baseOptions, title };

    const data = wohnungen.map((wohnung) => ({
        group: wohnung.bezeichnung,
        value: wohnung.wohnflaeche
    }));

    return { data, options };
}

export const months = [
    'Januar',
    'Februar',
    'März',
    'April',
    'Mai',
    'Juni',
    'Juli',
    'August',
    'September',
    'Oktober',
    'November',
    'Dezember'
];

export function walter_data_mieten(
    title: string,
    mieten: WalterMietzahlungListEntry[]
) {
    const sortedMieten = mieten.sort(
        (a, b) => a.kaltmieteZahlung - b.kaltmieteZahlung
    );

    const options = {
        ...baseOptions,
        title,
        axes: {
            left: {
                domain: [
                    sortedMieten[0].kaltmieteZahlung,
                    sortedMieten[sortedMieten.length - 1].kaltmieteZahlung
                ],
                mapsTo: 'value',
                scaleType: 'linear'
            },
            bottom: { mapsTo: 'date', scaleType: 'time' }
        }
    };

    const data = mieten.map((miete) => ({
        value: miete.kaltmieteZahlung,
        date: miete.betreffenderMonat
    }));

    return { options, data };
}

const colorGradient = [
    '#a3c68d',
    '#93bd7a',
    '#83b368',
    '#73aa56',
    '#63a143',
    '#2e7d32'
];

const heatMapOptions = {
    ...baseOptions,
    legend: {
        enabled: true
    },
    color: {
        gradient: {
            colors: colorGradient
        }
    },
    heatmap: {
        colorLegend: {
            type: 'quantize'
        }
    }
};

export function walter_data_rechnungentabelle(
    umlagen: WalterUmlageEntry[],
    year: number
): WalterDataConfigType {
    const options = {
        ...heatMapOptions,
        axes: {
            bottom: {
                title: 'Umlage',
                mapsTo: 'key',
                scaleType: 'labels',
                showTitle: false
            },
            left: {
                title: 'Wohnung',
                truncation: {
                    threshold: 999
                },
                mapsTo: 'group',
                scaleType: 'labels'
            }
        }
    };

    const data: WalterDataType = [];

    for (const umlage of umlagen) {
        const rechnungen = umlage.betriebskostenrechnungen.filter(
            (rechnung) => rechnung.betreffendesJahr === year
        );

        for (const wohnung of umlage.selectedWohnungen) {
            data.push({
                id: `${umlage.id}`,
                id2: `${umlage.typ.id}`,
                wohnungId: `${wohnung.id}`,
                group: wohnung.text,
                key: umlage.typ.text,
                value: rechnungen.reduce((pre, cur) => pre + cur.betrag, 0)
            });
        }
    }

    data.forEach((entry) => {
        entry.value =
            entry.value === null || (entry.value as number) > 0
                ? entry.value
                : undefined;
    });

    const wohnungen = new Set();
    data.forEach((entry) => wohnungen.add(entry.group));

    options.height = `${Math.max(wohnungen.size * 3, 20)}em`;

    return { data, options };
}

export function walter_data_nf(
    title: string,
    wohnungen: WalterWohnungEntry[]
): WalterDataConfigType {
    const options = { ...baseOptions, title };

    const data = wohnungen.map((wohnung) => ({
        group: wohnung.bezeichnung,
        value: wohnung.nutzflaeche
    }));

    return { data, options };
}

export function walter_data_mea(
    title: string,
    wohnungen: WalterWohnungEntry[]
): WalterDataConfigType {
    const options = { ...baseOptions, title };

    const data = wohnungen.map((wohnung) => ({
        group: wohnung.bezeichnung,
        value: wohnung.miteigentumsanteile
    }));

    return { data, options };
}

export function walter_data_ne(
    title: string,
    wohnungen: WalterWohnungEntry[]
): WalterDataConfigType {
    const options = { ...baseOptions, title };

    const data = wohnungen.map((wohnung) => ({
        group: wohnung.bezeichnung,
        value: wohnung.einheiten
    }));

    return { data, options };
}

/**
 * Soll- und Haben-Umsätze eines Buchungskontos je Monat als gruppierte
 * Balken — macht auf einen Blick sichtbar, wann sich beide Seiten decken.
 */
export function walter_data_soll_haben_monate(
    title: string,
    monatsSummen: WalterKontoMonatsSumme[]
): WalterDataConfigType {
    const options = {
        ...baseOptions,
        title,
        legend: { enabled: true },
        axes: {
            bottom: { mapsTo: 'key', scaleType: 'labels' },
            left: { mapsTo: 'value', scaleType: 'linear' }
        }
    };

    const data = monatsSummen.flatMap((monat) => {
        const key = `${`${monat.monat}`.padStart(2, '0')}/${monat.jahr}`;
        return [
            { group: 'Soll', key, value: monat.soll },
            { group: 'Haben', key, value: monat.haben }
        ];
    });

    return { data, options };
}

export type WalterDataConfigType = {
    data: WalterDataType;
    options: WalterDataOptionsType;
};

export type WalterDataOptionsType = {
    legend: { enabled: boolean };
    heatmap: unknown;
    height: string;
    title?: string;
    curve?: string;
    axes?: {
        bottom: unknown;
        left: unknown;
    };
    tooltip: {
        truncation: {
            threshold: number;
        };
    };
    [key: string]: unknown;
};

export type WalterDataType = WalterDataPoint[];

export type WalterDataPoint = {
    value: number | number[] | null | undefined;
    year?: number;
    group?: string; // row in heatmap
    id?: string; // id of vertrag
    id2?: string;
    wohnungId?: string;
    key?: string; // column in heatmap
    date?: string; // Date in Canadian Format
};
