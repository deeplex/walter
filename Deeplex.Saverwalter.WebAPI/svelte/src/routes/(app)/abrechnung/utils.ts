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
    override_result: boolean,
    fetchImpl: typeof fetch
) {
    const abrechnung = await create_abrechnung_word(
        vertragId,
        selectedYear,
        title,
        override_result,
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
    override_result: boolean,
    fetchImpl: typeof fetch
) {
    const abrechnung = await create_abrechnung_pdf(
        vertragId,
        selectedYear,
        title,
        override_result,
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
