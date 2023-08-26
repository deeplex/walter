import type {
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
    WalterMieteEntry,
    WalterVertragEntry,
    WalterWohnungEntry
} from '$walter/lib';
import { convertDateCanadian } from '$walter/services/utils';
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
        bottom: { mapsTo: 'value' },
        left: { mapsTo: 'group', scaleType: 'labels' }
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
    rechnungen: WalterBetriebskostenrechnungEntry[]
): WalterDataConfigType {
    const options = { ...baseOptions, title };

    const data = rechnungen.map((rechnung) => ({
        group: `${rechnung.umlage.text}`,
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

export function walter_data_miettabelle(
    vertraege: WalterVertragEntry[],
    year: number
): WalterDataConfigType {
    const colorGradient = [
        '#ffffff',
        '#f4f4f4',
        '#e4ebdf',
        '#d4e2c9',
        '#c4d9b3',
        '#b4d0a0',
        '#a3c68d',
        '#93bd7a',
        '#83b368',
        '#73aa56',
        '#63a143',
        '#2e7d32'
    ];

    const options = {
        ...baseOptions,
        height: `${vertraege.length * 2}em`,
        legend: {
            enabled: true
        },
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

    function vertragActive(begin: Date, end: Date | undefined, month: Date) {
        const began = begin <= month;
        const monthAfter = new Date(month.setMonth(month.getMonth()));
        const ended = end && end < monthAfter;

        return began && !ended;
    }

    const data: WalterDataType = [];
    for (const vertrag of vertraege) {
        const begin = new Date(
            new Date(vertrag.beginn).getFullYear(),
            new Date(vertrag.beginn).getMonth(),
            1
        );
        const end = vertrag.ende
            ? new Date(
                  new Date(vertrag.ende).getFullYear(),
                  new Date(vertrag.ende).getMonth(),
                  1
              )
            : undefined;

        for (let monthIndex = 0; monthIndex < months.length; ++monthIndex) {
            const eigentum = vertrag.versionen.every(
                (version) => version.grundmiete === 0
            );

            if (
                eigentum ||
                begin.getFullYear() > year ||
                (end && end.getFullYear() < year)
            ) {
                continue;
            }

            const monthDate = new Date(year, monthIndex, 1);

            const group = vertrag.wohnung.text;
            const key = months[monthIndex];

            const inactive = !vertragActive(begin, end, monthDate);
            const occupied = !!data.find(
                (entry) =>
                    entry.group === group &&
                    entry.key === key &&
                    entry.value !== null &&
                    !Array.isArray(entry.value) &&
                    entry.value >= 0
            );

            const doesNotHaveToPay = inactive && !occupied;

            if (doesNotHaveToPay) continue;

            const entry = {
                id: `${vertrag.id}`,
                year,
                value: 0,
                key,
                group
            };

            data.push(entry);
        }

        const relevantMieten = vertrag.mieten.filter(
            (miete) => new Date(miete.betreffenderMonat).getFullYear() === year
        );

        for (const miete of relevantMieten) {
            const group = vertrag.wohnung.text;
            const key = months[new Date(miete.betreffenderMonat).getMonth()];

            const previous = data.find(
                (entry) => entry.group === group && entry.key === key
            );

            if (previous) {
                previous.value =
                    ((previous.value as number) || 0) + miete.betrag;
            } else {
                // Should never happen...
                console.warn(
                    `Could not find value for ${key} ${year} for ${group}`
                );
            }
        }
    }

    return { options, data };
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
    heatmap: any;
    height: string;
    curve?: string;
    axes?: {
        bottom: any;
        left: any;
    };
    tooltip: {
        truncation: {
            threshold: number;
        };
    };
};

export type WalterDataType = {
    value: number | number[] | null;
    year?: number;
    group?: string;
    id?: string;
    key?: string;
    date?: string; // Date in Canadian Format
}[];
