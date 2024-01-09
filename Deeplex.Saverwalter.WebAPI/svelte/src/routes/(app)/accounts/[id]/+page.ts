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

import { WalterAccountEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterAccountEntry.ApiURL}/${params.id}`;

    const permissions = {
        read: true,
        update: true,
        remove: true
    };

    return {
        apiURL,
        fetchImpl: fetch,
        entry: {
            ...(await WalterAccountEntry.GetOne<WalterAccountEntry>(
                params.id,
                fetch
            )),
            permissions
        }
    };
};
