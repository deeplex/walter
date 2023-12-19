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
import {
    fileURL,
    walter_file_delete,
    walter_file_post
} from '$walter/services/files';
import type { WalterFile } from '$walter/types';
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
    file: WalterFile,
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
    const success = `Die Datei ${file.fileName} wurde erfolgreich ${target} kopiert.`;
    const failure = `Die Datei ${file.fileName} konnte nicht zu ${selectedEntry?.text} kopiert werden.`;

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
    file: WalterFile,
    fetchImpl: typeof fetch,
    newFileName: string
) {
    function successSubtitle() {
        return `Die Datei ${file.fileName} wurde zu ${newFileName} umbenannt.`;
    }
    function failureSubtitle() {
        return `Konnte ${file.fileName} nicht umbenennen. Ist die Dateiendung vielleicht nicht korrekt?`;
    }

    const failureTitle = 'Umbenennen fehlgeschlagen.';

    const renameToast = new WalterToastContent(
        'Umbenennen erfolgreich',
        failureTitle,
        successSubtitle,
        failureSubtitle
    );

    const newFile = new File([file.blob!], newFileName);
    const result = await walter_file_post(
        newFile,
        file.key,
        fetchImpl,
        renameToast
    );

    return result;
}

export async function moveImpl(
    file: WalterFile,
    fetchImpl: typeof fetch,
    selectedTable?: WalterPreviewCopyTable,
    selectedEntry?: WalterSelectionEntry
) {
    if (!selectedTable || (selectedTable.key !== 'stack' && !selectedEntry)) {
        return;
    }

    const success = `Die Datei ${file.fileName} wurde erfolgreich zu ${selectedEntry?.text} verschoben.`;
    function failureSubtitle() {
        return `Die Datei ${file.fileName} konnte nicht zu ${selectedEntry?.text} verschoben werden.`;
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
        const deleted = walter_file_delete(file, moveToast);
        if ((await deleted).status === 200) {
            return true;
        }
    }

    return false;
}

async function copyFile(
    file: WalterFile,
    selectedTable: WalterPreviewCopyTable,
    selectedEntry: WalterSelectionEntry,
    fetchImpl: typeof fetch,
    toast: WalterToastContent | undefined
) {
    if (!file.blob) {
        return false;
    }

    if (!selectedTable) {
        return false;
    }
    const fileURL =
        selectedTable.key === 'stack'
            ? `${selectedTable.fileURL('')}`
            : `${selectedTable.fileURL('' + selectedEntry?.id)}`;

    const success = walter_file_post(
        new File([file.blob], file.fileName),
        fileURL,
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
    fileURL: (id: string) => string;
    newPage: () => ConstructorOfATypedSvelteComponent;
};

export const tables: WalterPreviewCopyTable[] = [
    {
        value: 'Adressen',
        key: 'adressen',
        fetch: walter_selection.adressen,
        ApiURL: WalterAdresseEntry.ApiURL,
        fileURL: (id: string) => fileURL.adresse(id),
        newPage: () => WalterAdresse
    },
    {
        value: 'Betriebskostenrechnungen',
        key: 'betriebskostenrechnungen',
        fetch: walter_selection.betriebskostenrechnungen,
        ApiURL: WalterBetriebskostenrechnungEntry.ApiURL,
        fileURL: (id: string) => fileURL.betriebskostenrechnung(id),
        newPage: () => WalterBetriebskostenrechnung
    },
    {
        value: 'Erhaltungsaufwendungen',
        key: 'erhaltungsaufwendungen',
        fetch: walter_selection.erhaltungsaufwendungen,
        ApiURL: WalterErhaltungsaufwendungEntry.ApiURL,
        fileURL: (id: string) => fileURL.erhaltungsaufwendung(id),
        newPage: () => WalterErhaltungsaufwendung
    },
    {
        value: 'Kontakte',
        key: 'kontakte',
        fetch: walter_selection.kontakte,
        ApiURL: WalterKontaktEntry.ApiURL,
        fileURL: (id: string) => fileURL.kontakt(id),
        newPage: () => WalterKontakt
    },
    {
        value: 'Mieten',
        key: 'mieten',
        fetch: walter_selection.mieten,
        ApiURL: WalterMieteEntry.ApiURL,
        fileURL: (id: string) => fileURL.miete(id),
        newPage: () => WalterMiete
    },
    {
        value: 'Mietminderungen',
        key: 'mietminderungen',
        fetch: walter_selection.mietminderungen,
        ApiURL: WalterMietminderungEntry.ApiURL,
        fileURL: (id: string) => fileURL.mietminderung(id),
        newPage: () => WalterMietminderung
    },
    {
        value: 'Umlagen',
        key: 'umlagen',
        fetch: walter_selection.umlagen,
        ApiURL: WalterUmlageEntry.ApiURL,
        fileURL: (id: string) => fileURL.umlage(id),
        newPage: () => WalterUmlage
    },
    {
        value: 'Umlagetypen',
        key: 'umlagetypen',
        fetch: walter_selection.umlagetypen,
        ApiURL: WalterUmlagetypEntry.ApiURL,
        fileURL: (id: string) => fileURL.umlagetyp(id),
        newPage: () => WalterUmlagetyp
    },
    {
        value: 'Verträge',
        key: 'vertraege',
        fetch: walter_selection.vertraege,
        ApiURL: WalterVertragEntry.ApiURL,
        fileURL: (id: string) => fileURL.vertrag(id),
        newPage: () => WalterVertrag
    },
    // {
    //     value: 'Vertragversionen',
    //     key: 'vertragsversionen',
    //     fetch: walter_selection.vertragversionen,
    //     fileURL: 'fileURL.vertragversionen,
    // },
    {
        value: 'Wohnungen',
        key: 'wohnungen',
        fetch: walter_selection.wohnungen,
        ApiURL: WalterWohnungEntry.ApiURL,
        fileURL: (id: string) => fileURL.wohnung(id),
        newPage: () => WalterWohnung
    },
    {
        value: 'Zähler',
        key: 'zaehler',
        fetch: walter_selection.zaehler,
        ApiURL: WalterZaehlerEntry.ApiURL,
        fileURL: (id: string) => fileURL.zaehler(id),
        newPage: () => WalterZaehler
    },
    {
        value: 'Zählerstände',
        key: 'zaehlerstaende',
        fetch: walter_selection.zaehlerstaende,
        ApiURL: WalterZaehlerstandEntry.ApiURL,
        fileURL: (id: string) => fileURL.zaehlerstand(id),
        newPage: () => WalterZaehlerstand
    },
    {
        value: 'Ablagestapel',
        key: 'stack',
        fetch: () => Promise.resolve(),
        ApiURL: '',
        fileURL: () => fileURL.stack,
        newPage: () =>
            undefined as unknown as ConstructorOfATypedSvelteComponent
    }
];
