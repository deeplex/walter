import { WalterToastContent } from '$walter/lib';
import {
    create_abrechnung_pdf,
    create_abrechnung_word
} from '$walter/services/abrechnung';
import {
    create_walter_s3_file_from_file,
    walter_s3_post
} from '$walter/services/s3';

export async function create_word_doc(
    vertragId: string,
    selectedYear: number,
    title: string,
    S3URL: string,
    fetchImpl: typeof fetch
) {
    const abrechnung = await create_abrechnung_word(
        vertragId,
        selectedYear,
        title
    );
    if (abrechnung instanceof File) {
        return create_abrechnung(abrechnung, S3URL, fetchImpl);
    }
}

export async function create_pdf_doc(
    vertragId: string,
    selectedYear: number,
    title: string,
    S3URL: string,
    fetchImpl: typeof fetch
) {
    const abrechnung = await create_abrechnung_pdf(
        vertragId,
        selectedYear,
        title
    );
    if (abrechnung instanceof File) {
        return create_abrechnung(abrechnung, S3URL, fetchImpl);
    }
}

async function create_abrechnung(
    abrechnung: File,
    S3URL: string,
    fetchImpl: typeof fetch
) {
    const file = create_walter_s3_file_from_file(abrechnung, S3URL);

    const toast = new WalterToastContent(
        'Hochladen erfolgreich',
        'Hochladen fehlgeschlagen',
        () => `Die Datei: ${file.FileName} wurde erfolgreich hochgeladen`,
        () => `Die Datei: ${file.FileName} konnte nicht hochgeladen werden.`
    );

    var response = await walter_s3_post(
        new File([abrechnung], file.FileName),
        S3URL,
        fetchImpl,
        toast
    );

    if (response.status === 200) {
        return file;
    }
}
