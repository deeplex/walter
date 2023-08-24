import type {
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
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

const months = [
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

export function walter_data_miettabelle(
    vertraege: WalterVertragEntry[],
    year: number
): WalterDataConfigType {
    const options = {
        ...baseOptions,
        legend: {
            enabled: true
        },
        axes: {
            bottom: {
                mapsTo: 'key',
                scaleType: 'labels'
            },
            left: {
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

    const prefilled = vertraege.flatMap((vertrag) => {
        return months.map((month) => ({
            key: month,
            value: 0,
            group: vertrag.wohnung.text
        }));
    });

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
    group: string;
    value: number | number[];
    key?: string;
    date?: string; // Date in Canadian Format
}[];
