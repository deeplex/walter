import { WalterToastContent } from '$walter/lib';
import {
    create_abrechnung_pdf,
    create_abrechnung_word,
    loadAbrechnung
} from '$walter/services/abrechnung';
import { fileURL, walter_file_post } from '$walter/services/files';
import { WalterFile } from '$walter/lib/WalterFile';

export async function create_word_doc(
    vertragId: number,
    selectedYear: number,
    title: string,
    fetchImpl: typeof fetch
) {
    const abrechnung = await create_abrechnung_word(
        vertragId,
        selectedYear,
        title,
        fetchImpl
    );
    if (abrechnung instanceof File) {
        const fileUrl = fileURL.vertrag(`${vertragId}`);
        return create_abrechnung(abrechnung, fileUrl, fetchImpl);
    }
}

export async function create_pdf_doc(
    vertragId: number,
    selectedYear: number,
    title: string,
    fetchImpl: typeof fetch
) {
    const abrechnung = await create_abrechnung_pdf(
        vertragId,
        selectedYear,
        title,
        fetchImpl
    );
    if (abrechnung instanceof File) {
        const fileUrl = fileURL.vertrag(`${vertragId}`);
        return create_abrechnung(abrechnung, fileUrl, fetchImpl);
    }
}

async function create_abrechnung(
    abrechnung: File,
    fileURL: string,
    fetchImpl: typeof fetch
) {
    const file = WalterFile.fromFile(abrechnung, fileURL);

    const toast = new WalterToastContent(
        'Hochladen erfolgreich',
        'Hochladen fehlgeschlagen',
        () => `Die Datei: ${file.fileName} wurde erfolgreich hochgeladen`,
        () => `Die Datei: ${file.fileName} konnte nicht hochgeladen werden.`
    );

    const response = await walter_file_post(
        new File([abrechnung], file.fileName),
        fileURL,
        fetchImpl,
        toast
    );

    if (response.status === 200) {
        return file;
    }
}

export function updatePreview(
    vertragId: number | null,
    selectedYear: number | null,
    fetchImpl: typeof fetch
) {
    let abrechnung;

    if (vertragId && selectedYear) {
        abrechnung = loadAbrechnung(vertragId, selectedYear, fetchImpl);
    } else {
        abrechnung = Promise.resolve(undefined);
    }

    return abrechnung;
}
