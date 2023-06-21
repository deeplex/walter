import { walter_selection } from '$walter/services/requests';

export type WalterPreviewCopyTable = {
    value: string;
    key: string;
    fetch: (
        fetchImpl: (
            input: RequestInfo | URL,
            init?: RequestInit | undefined
        ) => Promise<Response>
    ) => Promise<any>;
    S3URL: string;
};

export const tables: WalterPreviewCopyTable[] = [
    {
        value: 'Adressen',
        key: 'adressen',
        fetch: walter_selection.adressen,
        S3URL: 'adressen'
    },
    {
        value: 'Betriebskostenrechnungen',
        key: 'betriebskostenrechnungen',
        fetch: walter_selection.betriebskostenrechnungen,
        S3URL: 'betriebskostenrechnungen'
    },
    {
        value: 'Erhaltungsaufwendungen',
        key: 'erhaltungsaufwendungen',
        fetch: walter_selection.erhaltungsaufwendungen,
        S3URL: 'erhaltungsaufwendungen'
    },
    {
        value: 'Natürliche Personen',
        key: 'natuerlichepersonen',
        fetch: walter_selection.natuerlichePersonen,
        S3URL: 'kontakte/nat'
    },
    {
        value: 'Juristische Personen',
        key: 'juristischepersonen',
        fetch: walter_selection.juristischePersonen,
        S3URL: 'kontakte/jur'
    },
    {
        value: 'Mieten',
        key: 'mieten',
        fetch: walter_selection.mieten,
        S3URL: 'mieten'
    },
    {
        value: 'Mietminderungen',
        key: 'mietminderungen',
        fetch: walter_selection.mietminderungen,
        S3URL: 'mietminderungen'
    },
    {
        value: 'Umlagen',
        key: 'umlagen',
        fetch: walter_selection.umlagen,
        S3URL: 'umlagen'
    },
    {
        value: 'Verträge',
        key: 'vertraege',
        fetch: walter_selection.vertraege,
        S3URL: 'vertraege'
    },
    // {
    //     value: 'Vertragversionen',
    //     key: 'vertragsversionen',
    //     fetch: walter_selection.vertragversionen,
    //     S3URL: 'vertragsversionen',
    // },
    {
        value: 'Wohnungen',
        key: 'wohnungen',
        fetch: walter_selection.wohnungen,
        S3URL: 'wohnungen'
    },
    {
        value: 'Zähler',
        key: 'zaehler',
        fetch: walter_selection.zaehler,
        S3URL: 'zaehler'
    },
    {
        value: 'Zählerstände',
        key: 'zaehlerstaende',
        fetch: walter_selection.zaehlerstaende,
        S3URL: 'zaehlerstaende'
    }
];
