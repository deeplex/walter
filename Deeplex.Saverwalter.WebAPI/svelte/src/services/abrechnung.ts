// Copyright (c) 2023-2024 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
    override_result: boolean,
    fetchImpl: typeof fetch
) {
    const apiURL = `/api/betriebskostenabrechnung/${vertrag_id}/${jahr}/word_document`;
    const fileName = `${override_result ? '' : 'Entwurf: '}Abrechnung ${jahr} - ${fileNameBase}.docx`;
    return create_abrechnung_file(apiURL, fileName, override_result, fetchImpl);
}

export function create_abrechnung_pdf(
    vertrag_id: number,
    jahr: number,
    fileNameBase: string,
    override_result: boolean,
    fetchImpl: typeof fetch
) {
    const apiURL = `/api/betriebskostenabrechnung/${vertrag_id}/${jahr}/pdf_document`;
    const fileName = `${override_result ? '' : 'Entwurf: '}Abrechnung ${jahr} - ${fileNameBase}.pdf`;
    return create_abrechnung_file(apiURL, fileName, override_result, fetchImpl);
}

async function create_abrechnung_file(
    apiURL: string,
    fileName: string,
    override_result: boolean,
    fetchImpl: typeof fetch
) {
    const fetchOptions = {
        method: override_result ? 'POST' : 'GET',
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
