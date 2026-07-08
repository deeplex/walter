// Copyright (c) 2023-2026 Kai Lawrence
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

import { WalterWohnungEntry, WalterWohnungVersionEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterWohnungVersionEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.wohnungversion(params.id);

    const entry = WalterWohnungVersionEntry.GetOne<WalterWohnungVersionEntry>(
        params.id,
        fetch
    );
    const wohnung = WalterWohnungEntry.GetOne<WalterWohnungEntry>(
        `${(await entry).wohnung.id}`,
        fetch
    );

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        wohnung,
        entry
    };
};
