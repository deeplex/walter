import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
import { walter_fetch, walter_get } from './requests';
import { finish_file_post } from './files';

const headers = {
    'Content-Type': 'application/octet-stream'
};

export function create_abrechnung_word(
    vertrag_id: number,
    jahr: number,
    fileNameBase: string,
    fetchImpl: typeof fetch
) {
    const apiURL = `/api/betriebskostenabrechnung/${vertrag_id}/${jahr}/word_document`;
    const fileName = `Abrechnung ${jahr} - ${fileNameBase}.docx`;
    return create_abrechnung_file(apiURL, fileName, fetchImpl);
}

export function create_abrechnung_pdf(
    vertrag_id: number,
    jahr: number,
    fileNameBase: string,
    fetchImpl: typeof fetch
) {
    const apiURL = `/api/betriebskostenabrechnung/${vertrag_id}/${jahr}/pdf_document`;
    const fileName = `Abrechnung ${jahr} - ${fileNameBase}.pdf`;
    return create_abrechnung_file(apiURL, fileName, fetchImpl);
}

async function create_abrechnung_file(
    apiURL: string,
    fileName: string,
    fetchImpl: typeof fetch
) {
    const fetchOptions = {
        method: 'GET',
        headers
    };
    const response = await walter_fetch(fetchImpl, apiURL, fetchOptions);

    if (response.status === 200) {
        return response.blob().then((e) => new File([e], fileName));
    } else {
        return finish_file_post(response);
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
