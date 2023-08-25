import type {
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
    WalterMieteEntry,
    WalterVertragEntry,
    WalterWohnungEntry
} from '$walter/lib';
import type { WalterRechnungEntry } from '$walter/types';

const baseOptions = {
    legend: { enabled: false },
    height: '400px'
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

    const prefilled: WalterDataType = [];
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
            const monthDate = new Date(year, monthIndex, 1);

            const group = vertrag.wohnung.text;
            const key = months[monthIndex];

            const eigentum = vertrag.versionen.every(
                (version) => version.grundmiete === 0
            );
            const inactive = !vertragActive(begin, end, monthDate);
            const occupied = prefilled.some(
                (entry) =>
                    entry.group === group &&
                    entry.key === key &&
                    entry.value !== null &&
                    !Array.isArray(entry.value) &&
                    entry.value >= 0
            );

            const doesNotHaveToPay =
                (!inactive && eigentum) || (inactive && !occupied);

            prefilled.push({
                value: doesNotHaveToPay ? null : 0,
                key,
                group
            });
        }
    }

    const data = [
        ...prefilled,
        ...vertraege.flatMap((vertrag) => {
            return vertrag.mieten
                .filter(
                    (miete) =>
                        new Date(miete.betreffenderMonat).getFullYear() === year
                )
                .map((miete) => ({
                    key: months[new Date(miete.betreffenderMonat).getMonth()],
                    value: miete.betrag,
                    group: vertrag.wohnung.text
                }));
        })
    ];

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
};

export type WalterDataType = {
    value: number | number[] | null;
    group?: string;
    id?: string;
    key?: string;
    date?: string; // Date in Canadian Format
}[];
