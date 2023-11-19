import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
import { walter_fetch, walter_get } from './requests';
import { finish_s3_post } from './s3';

const headers = {
    'Content-Type': 'application/octet-stream'
};

export function create_abrechnung_word(
    vertrag_id: number,
    jahr: number,
    fileNameBase: string
) {
    const apiURL = `/api/betriebskostenabrechnung/${vertrag_id}/${jahr}/word_document`;
    const fileName = `Abrechnung ${jahr} - ${fileNameBase}.docx`;
    return create_abrechnung_file(apiURL, fileName);
}

export function create_abrechnung_pdf(
    vertrag_id: number,
    jahr: number,
    fileNameBase: string
) {
    const apiURL = `/api/betriebskostenabrechnung/${vertrag_id}/${jahr}/pdf_document`;
    const fileName = `Abrechnung ${jahr} - ${fileNameBase}.pdf`;
    return create_abrechnung_file(apiURL, fileName);
}

async function create_abrechnung_file(apiURL: string, fileName: string) {
    const fetchOptions = {
        method: 'GET',
        headers
    };
    const response = await walter_fetch(fetch, apiURL, fetchOptions);

    if (response.status === 200) {
        return response.blob().then((e) => new File([e], fileName));
    } else {
        return finish_s3_post(response);
    }
}

export function loadAbrechnung(
    vertragId: number,
    year: number,
    fetchImpl: typeof fetch
) {
    const abrechnungURL = `/api/betriebskostenabrechnung/${vertragId}/${year}`;

    const abrechnung = walter_get(
        abrechnungURL,
        fetchImpl
    ) as Promise<WalterBetriebskostenabrechnungEntry>;

    return abrechnung;
}
