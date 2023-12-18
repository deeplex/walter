import {
    WalterToastContent,
    type WalterSelectionEntry,
    WalterAdresseEntry,
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
    WalterKontaktEntry,
    WalterMieteEntry,
    WalterMietminderungEntry,
    WalterUmlageEntry,
    WalterUmlagetypEntry,
    WalterVertragEntry,
    WalterWohnungEntry,
    WalterZaehlerEntry,
    WalterZaehlerstandEntry
} from '$walter/lib';
import { walter_selection } from '$walter/services/requests';
import { S3URL, walter_s3_delete, walter_s3_post } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import {
    WalterAdresse,
    WalterBetriebskostenrechnung,
    WalterErhaltungsaufwendung,
    WalterKontakt,
    WalterMiete,
    WalterMietminderung,
    WalterUmlage,
    WalterUmlagetyp,
    WalterVertrag,
    WalterWohnung,
    WalterZaehler,
    WalterZaehlerstand
} from '..';

export async function copyImpl(
    file: WalterS3File,
    fetchImpl: typeof fetch,
    selectedTable?: WalterPreviewCopyTable,
    selectedEntry?: WalterSelectionEntry
) {
    if (!selectedTable || (selectedTable.key !== 'stack' && !selectedEntry)) {
        return;
    }

    const target =
        selectedTable.key === 'stack'
            ? 'auf den Ablagestapel'
            : `zu ${selectedEntry?.text}`;
    const success = `Die Datei ${file.FileName} wurde erfolgreich ${target} kopiert.`;
    const failure = `Die Datei ${file.FileName} konnte nicht zu ${selectedEntry?.text} kopiert werden.`;

    const copyToast = new WalterToastContent(
        'Kopieren erfolgreich',
        'Kopieren fehlgeschlagen',
        () => success,
        () => failure
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

export async function renameImpl(
    file: WalterS3File,
    fetchImpl: typeof fetch,
    newFileName: string
) {
    function successSubtitle() {
        return `Die Datei ${file.FileName} wurde zu ${newFileName} umbenannt.`;
    }
    function failureSubtitle() {
        return `Konnte ${file.FileName} nicht umbenennen. Ist die Dateiendung vielleicht nicht korrekt?`;
    }

    const failureTitle = 'Umbenennen fehlgeschlagen.';

    const renameToast = new WalterToastContent(
        'Umbenennen erfolgreich',
        failureTitle,
        successSubtitle,
        failureSubtitle
    );

    const newFile = new File([file.Blob!], newFileName);
    const result = await walter_s3_post(
        newFile,
        file.Key,
        fetchImpl,
        renameToast
    );

    return result;
}

export async function moveImpl(
    file: WalterS3File,
    fetchImpl: typeof fetch,
    selectedTable?: WalterPreviewCopyTable,
    selectedEntry?: WalterSelectionEntry
) {
    if (!selectedTable || (selectedTable.key !== 'stack' && !selectedEntry)) {
        return;
    }

    const success = `Die Datei ${file.FileName} wurde erfolgreich zu ${selectedEntry?.text} verschoben.`;
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
        () => success,
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
    const S3URL =
        selectedTable.key === 'stack'
            ? `${selectedTable.S3URL('')}`
            : `${selectedTable.S3URL('' + selectedEntry?.id)}`;

    console.log(selectedTable);
    console.log(S3URL);

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
    ) => Promise<unknown>;
    ApiURL: string;
    S3URL: (id: string) => string;
    newPage: () => ConstructorOfATypedSvelteComponent;
};

export const tables: WalterPreviewCopyTable[] = [
    {
        value: 'Adressen',
        key: 'adressen',
        fetch: walter_selection.adressen,
        ApiURL: WalterAdresseEntry.ApiURL,
        S3URL: (id: string) => S3URL.adresse(id),
        newPage: () => WalterAdresse
    },
    {
        value: 'Betriebskostenrechnungen',
        key: 'betriebskostenrechnungen',
        fetch: walter_selection.betriebskostenrechnungen,
        ApiURL: WalterBetriebskostenrechnungEntry.ApiURL,
        S3URL: (id: string) => S3URL.betriebskostenrechnung(id),
        newPage: () => WalterBetriebskostenrechnung
    },
    {
        value: 'Erhaltungsaufwendungen',
        key: 'erhaltungsaufwendungen',
        fetch: walter_selection.erhaltungsaufwendungen,
        ApiURL: WalterErhaltungsaufwendungEntry.ApiURL,
        S3URL: (id: string) => S3URL.erhaltungsaufwendung(id),
        newPage: () => WalterErhaltungsaufwendung
    },
    {
        value: 'Kontakte',
        key: 'kontakte',
        fetch: walter_selection.kontakte,
        ApiURL: WalterKontaktEntry.ApiURL,
        S3URL: (id: string) => S3URL.kontakt(id),
        newPage: () => WalterKontakt
    },
    {
        value: 'Mieten',
        key: 'mieten',
        fetch: walter_selection.mieten,
        ApiURL: WalterMieteEntry.ApiURL,
        S3URL: (id: string) => S3URL.miete(id),
        newPage: () => WalterMiete
    },
    {
        value: 'Mietminderungen',
        key: 'mietminderungen',
        fetch: walter_selection.mietminderungen,
        ApiURL: WalterMietminderungEntry.ApiURL,
        S3URL: (id: string) => S3URL.mietminderung(id),
        newPage: () => WalterMietminderung
    },
    {
        value: 'Umlagen',
        key: 'umlagen',
        fetch: walter_selection.umlagen,
        ApiURL: WalterUmlageEntry.ApiURL,
        S3URL: (id: string) => S3URL.umlage(id),
        newPage: () => WalterUmlage
    },
    {
        value: 'Umlagetypen',
        key: 'umlagetypen',
        fetch: walter_selection.umlagetypen,
        ApiURL: WalterUmlagetypEntry.ApiURL,
        S3URL: (id: string) => S3URL.umlagetyp(id),
        newPage: () => WalterUmlagetyp
    },
    {
        value: 'Verträge',
        key: 'vertraege',
        fetch: walter_selection.vertraege,
        ApiURL: WalterVertragEntry.ApiURL,
        S3URL: (id: string) => S3URL.vertrag(id),
        newPage: () => WalterVertrag
    },
    // {
    //     value: 'Vertragversionen',
    //     key: 'vertragsversionen',
    //     fetch: walter_selection.vertragversionen,
    //     S3URL: 'S3URL.vertragversionen,
    // },
    {
        value: 'Wohnungen',
        key: 'wohnungen',
        fetch: walter_selection.wohnungen,
        ApiURL: WalterWohnungEntry.ApiURL,
        S3URL: (id: string) => S3URL.wohnung(id),
        newPage: () => WalterWohnung
    },
    {
        value: 'Zähler',
        key: 'zaehler',
        fetch: walter_selection.zaehler,
        ApiURL: WalterZaehlerEntry.ApiURL,
        S3URL: (id: string) => S3URL.zaehler(id),
        newPage: () => WalterZaehler
    },
    {
        value: 'Zählerstände',
        key: 'zaehlerstaende',
        fetch: walter_selection.zaehlerstaende,
        ApiURL: WalterZaehlerstandEntry.ApiURL,
        S3URL: (id: string) => S3URL.zaehlerstand(id),
        newPage: () => WalterZaehlerstand
    },
    {
        value: 'Ablagestapel',
        key: 'stack',
        fetch: () => Promise.resolve(),
        ApiURL: '',
        S3URL: () => S3URL.stack,
        newPage: () => undefined
    }
];
