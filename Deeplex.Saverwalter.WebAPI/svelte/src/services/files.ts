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

import { addToast } from '$walter/store';
import { WalterFile } from '$walter/lib/WalterFile';
import { walter_delete, walter_fetch, walter_get } from './requests';
import type { WalterToastContent } from '../lib/WalterToastContent';
import {
    WalterAdresseEntry,
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
    WalterKontaktEntry,
    WalterMieteEntry,
    WalterMietminderungEntry,
    WalterUmlageEntry,
    WalterUmlagetypEntry,
    WalterVertragEntry,
    WalterVertragVersionEntry,
    WalterWohnungEntry,
    WalterZaehlerEntry,
    WalterZaehlerstandEntry
} from '$walter/lib';

export const fileURL = {
    adresse: (id: string) => `${WalterAdresseEntry.ApiURL}/${id}/files`,
    betriebskostenrechnung: (id: string) =>
        `${WalterBetriebskostenrechnungEntry.ApiURL}/${id}/files`,
    erhaltungsaufwendung: (id: string) =>
        `${WalterErhaltungsaufwendungEntry.ApiURL}/${id}/files`,
    miete: (id: string) => `${WalterMieteEntry.ApiURL}/${id}/files`,
    mietminderung: (id: string) =>
        `${WalterMietminderungEntry.ApiURL}/${id}/files`,
    kontakt: (id: string) => `${WalterKontaktEntry.ApiURL}/${id}/files`,
    umlage: (id: string) => `${WalterUmlageEntry.ApiURL}/${id}/files`,
    umlagetyp: (id: string) => `${WalterUmlagetypEntry.ApiURL}/${id}/files`,
    vertrag: (id: string) => `${WalterVertragEntry.ApiURL}/${id}/files`,
    vertragversion: (id: string) =>
        `${WalterVertragVersionEntry.ApiURL}/${id}/files`,
    wohnung: (id: string) => `${WalterWohnungEntry.ApiURL}/${id}/files`,
    zaehler: (id: string) => `${WalterZaehlerEntry.ApiURL}/${id}/files`,
    zaehlerstand: (id: string) =>
        `${WalterZaehlerstandEntry.ApiURL}/${id}/files`,
    stack: `/api/user/files`
};

export const walter_file_post = (
    file: File,
    url: string,
    fetchImpl: typeof fetch,
    toast?: WalterToastContent
) =>
    walter_fetch(fetchImpl, `${url}/${file.name}`, {
        method: 'PUT',
        headers: {
            // Ignored, due to header being replaced in walter_fetch
            'Content-Type': `${file.type}`
        },
        body: file
    }).then((e) => finish_file_post(e, toast));

export async function finish_file_post(
    e: Response,
    toast?: WalterToastContent
) {
    toast && addToast(toast, e.status === 200);
    return e;
}

export const walter_file_get = (fileURL: string): Promise<unknown> =>
    walter_fetch(fetch, fileURL, { method: 'GET' }).then((e) => e.blob());

export async function walter_file_delete(
    file: WalterFile,
    toast?: WalterToastContent
) {
    return walter_delete(file.key, toast);
}

export function download_file_blob(blob: Blob, fileName: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

export async function walter_get_files(
    fileURL: string,
    fetchImpl: typeof fetch
): Promise<WalterFile[]> {
    const fetched = (await walter_get(
        fileURL,
        fetchImpl
    )) as unknown as WalterFile[];

    const parsed = fetched.map(
        (e) =>
            new WalterFile(
                e.fileName,
                e.key,
                e.lastModified,
                e.size,
                e.blob,
                e.type
            )
    );

    return parsed;
}
