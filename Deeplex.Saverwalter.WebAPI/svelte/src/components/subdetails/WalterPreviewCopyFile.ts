import { WalterToastContent, type WalterSelectionEntry } from '$walter/lib';
import { walter_selection } from '$walter/services/requests';
import { walter_s3_delete, walter_s3_post } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';

export async function copyImpl(
    file: WalterS3File,
    selectedTable: WalterPreviewCopyTable,
    selectedEntry: WalterSelectionEntry,
    fetchImpl: typeof fetch
) {
    const copyToast = new WalterToastContent(
        'Kopieren erfolgreich',
        'Kopieren fehlgeschlagen',
        () =>
            `Die Datei ${file.FileName} wurde erfolgreich zu ${selectedEntry?.text} kopiert.`,
        () =>
            `Die Datei ${file.FileName} konnte nicht zu ${selectedEntry?.text} kopiert werden.`
    );

    const copied = copyFile(
        file,
        selectedTable!,
        selectedEntry!,
        fetchImpl,
        copyToast
    );

    return await copied;
}

export async function moveImpl(
    file: WalterS3File,
    selectedTable: WalterPreviewCopyTable,
    selectedEntry: WalterSelectionEntry,
    fetchImpl: typeof fetch
) {
    function failureSubtitle() {
        return `Die Datei ${file.FileName} konnte nicht zu ${selectedEntry?.text} verschoben werden.`;
    }
    const failureTitle = 'Verschieben fehlgeschlagen';
    const copyToast = new WalterToastContent(
        undefined,
        failureTitle,
        undefined,
        failureSubtitle
    );
    const moveToast = new WalterToastContent(
        'Verschieben erfolgreich',
        failureTitle,
        () =>
            `Die Datei ${file.FileName} wurde erfolgreich zu ${selectedEntry?.text} verschoben.`,
        failureSubtitle
    );

    const copied = copyFile(
        file,
        selectedTable!,
        selectedEntry!,
        fetchImpl,
        copyToast
    );

    if (await copied) {
        const deleted = walter_s3_delete(file, moveToast);
        if ((await deleted).status === 200) {
            return true;
        }
    }

    return false;
}

async function copyFile(
    file: WalterS3File,
    selectedTable: WalterPreviewCopyTable,
    selectedEntry: WalterSelectionEntry,
    fetchImpl: typeof fetch,
    toast: WalterToastContent | undefined
) {
    if (!file.Blob) {
        return false;
    }

    if (!selectedTable) {
        return false;
    }
    const S3URL = `${selectedTable.S3URL}/${selectedEntry?.id}`;

    const success = walter_s3_post(
        new File([file.Blob], file.FileName),
        S3URL,
        fetchImpl,
        toast
    );

    return (await success).status === 200;
}

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
