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

import type {
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
    WalterMieteEntry,
    WalterUmlageEntry,
    WalterVertragEntry,
    WalterWohnungEntry
} from '$walter/lib';
import type { WalterRechnungEntry } from '$walter/types';

const baseOptions = {
    legend: { enabled: false },
    height: '400px',
    tooltip: {
        truncation: {
            threshold: 999
        }
    }
} as WalterDataOptionsType;

const lineAxes = {
    axes: {
        bottom: { mapsTo: 'value', includeZero: false },
        left: { mapsTo: 'group', scaleType: 'labels', includeZero: false }
    }
};

export function walter_data_rechnungen(
    title: string,
    rechnungen: WalterRechnungEntry[]
): WalterDataConfigType {
    const options = {
        ...baseOptions,
        title
    };
    const data = rechnungen.map((e) => ({
        group: e.typ,
        value: e.gesamtBetrag
    }));

    return { data, options };
}

export function walter_data_rechnungen_pairs(
    title: string,
    rechnungen: WalterRechnungEntry[]
): WalterDataConfigType {
    const options = {
        ...baseOptions,
        ...lineAxes,
        title
    };

    const data = rechnungen.map((rechnung) => ({
        group: rechnung.typ,
        value: [rechnung.betragLetztesJahr, rechnung.gesamtBetrag]
    }));

    return { data, options };
}

export function walter_data_rechnungen_diff(
    title: string,
    rechnungen: WalterRechnungEntry[]
): WalterDataConfigType {
    const options = {
        ...baseOptions,
        ...lineAxes,
        title
    };

    const data = rechnungen.map((rechnung) => ({
        group: rechnung.typ,
        value: rechnung.gesamtBetrag - rechnung.betragLetztesJahr
    }));

    return { data, options };
}

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
        group: `${aufwendung.aussteller.text}`,
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

export function walter_data_mieten(title: string, mieten: WalterMieteEntry[]) {
    const sortedMieten = mieten.sort((a, b) => a.betrag - b.betrag);

    const options = {
        ...baseOptions,
        title,
        axes: {
            left: {
                domain: [
                    sortedMieten[0].betrag,
                    sortedMieten[sortedMieten.length - 1].betrag
                ],
                mapsTo: 'value',
                scaleType: 'linear'
            },
            bottom: { mapsTo: 'date', scaleType: 'time' }
        }
    };

    const data = mieten.map((miete) => ({
        value: miete.betrag,
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

const walter_data_miettabelle_options = {
    ...heatMapOptions,
    axes: {
        bottom: {
            title: 'Monat',
            mapsTo: 'key',
            scaleType: 'labels'
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

function vertragActiveMonth(begin: Date, end: Date | undefined, month: Date) {
    const began = begin <= month;
    const monthAfter = new Date(month.setMonth(month.getMonth()));
    const ended = end && end < monthAfter;

    return began && !ended;
}

function vertragActiveYear(begin: Date, end: Date | undefined, year: number) {
    const began = begin.getFullYear() <= year;
    const ended = end && end.getFullYear() < year;

    return began && !ended;
}

function getVertragBeginn(vertrag: WalterVertragEntry): Date {
    return new Date(
        new Date(vertrag.beginn).getFullYear(),
        new Date(vertrag.beginn).getMonth(),
        1
    );
}

function getVertragEnde(vertrag: WalterVertragEntry): Date | undefined {
    return vertrag.ende
        ? new Date(
            new Date(vertrag.ende).getFullYear(),
            new Date(vertrag.ende).getMonth(),
            1
        )
        : undefined;
}

function fillDataWithMieten(
    vertrag: WalterVertragEntry,
    data: WalterDataType,
    year: number
) {
    const mietenInThisYear = vertrag.mieten.filter(
        (miete) => new Date(miete.betreffenderMonat).getFullYear() === year
    );

    for (const miete of mietenInThisYear) {
        const group = vertrag.wohnung.text;
        const key = months[new Date(miete.betreffenderMonat).getMonth()];

        const previous = data.find(
            (entry) => entry.group === group && entry.key === key
        );

        if (!previous) {
            console.warn(
                `Could not find value for ${key} ${year} for ${group}`
            );
            return;
        }

        previous.value = ((previous.value as number) || 0) + miete.betrag;
    }
}

export function walter_data_miettabelle(
    vertraege: WalterVertragEntry[],
    year: number
): WalterDataConfigType {
    const data: WalterDataType = [];

    // go through each vertrag and check if it should get miete for the specific month
    for (const vertrag of vertraege) {
        const begin = getVertragBeginn(vertrag);
        const end = getVertragEnde(vertrag);

        // Skip vertrag if it is not active in the given year
        if (!vertragActiveYear(begin, end, year)) {
            continue;
        }
        // Skip vertrag if it has no grundmiete
        if (vertrag.versionen.every((version) => version.grundmiete === 0)) {
            continue;
        }

        // Iterate through each month of the year
        for (let monthIndex = 0; monthIndex < months.length; ++monthIndex) {
            const monthDate = new Date(year, monthIndex, 1);
            // Skip vertrag if it is not active in the given month
            if (!vertragActiveMonth(begin, end, monthDate)) {
                continue;
            }

            // Every entry that is eligible for miete gets an entry in the data array (value = 0)
            const entry = {
                id: `${vertrag.id}`,
                year,
                value: 0,
                key: months[monthIndex],
                group: vertrag.wohnung.text
            };

            data.push(entry);
        }

        fillDataWithMieten(vertrag, data, year);
    }

    data.forEach((entry) => {
        entry.value =
            entry.value === null || (entry.value as number) > 0
                ? entry.value
                : undefined;
    });

    // Get number of unique Wohnungen to scale table
    const wohnungen = new Set();
    data.forEach((entry) => wohnungen.add(entry.group));
    const height = `${wohnungen.size * 3}em`;

    return {
        data,
        options: { ...walter_data_miettabelle_options, height }
    };
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

export type WalterDataConfigType = {
    data: WalterDataType;
    options: WalterDataOptionsType;
};

export type WalterDataOptionsType = {
    legend: { enabled: boolean };
    heatmap: unknown;
    height: string;
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
};

export type WalterDataType = WalterDataPoint[];

export type WalterDataPoint = {
    value: number | number[] | null | undefined;
    year?: number;
    group?: string; // row in heatmap
    id?: string; // id of vertrag
    id2?: string;
    key?: string; // column in heatmap
    date?: string; // Date in Canadian Format
};
