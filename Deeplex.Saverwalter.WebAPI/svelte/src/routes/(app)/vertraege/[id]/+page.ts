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

import { WalterMietzahlungListEntry, WalterMietzahlungApiURL, WalterVertragEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import { walter_get } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterVertragEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.vertrag(params.id);

    const mietzahlungenPromise = (
        walter_get(`${WalterMietzahlungApiURL}/${params.id}`, fetch) as Promise<
            WalterMietzahlungListEntry[]
        >
    )
        .then((list) => list.map(WalterMietzahlungListEntry.fromJson))
        .catch(() => [] as WalterMietzahlungListEntry[]);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterVertragEntry.GetOne<WalterVertragEntry>(params.id, fetch),
        mietzahlungen: mietzahlungenPromise
    };
};
