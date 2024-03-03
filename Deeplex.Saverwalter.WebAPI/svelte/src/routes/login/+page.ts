// Copyright (c) 2023-2024 Henrik S. Gaßmann, Kai Lawrence
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

import type { PageLoad } from './$types';
import { browser } from '$app/environment';
import { walter_goto } from '$walter/services/utils';

export const load: PageLoad = async ({ fetch }) => {
    if (!browser) {
        return {
            fetch
        };
    }

    const accessToken = (
        await import('$walter/services/auth')
    ).getAccessToken();
    if (accessToken == null) {
        return {
            fetch
        };
    }
    const response = await fetch('/api/user/refresh-token', {
        method: 'POST',
        headers: {
            Authorization: `X-WalterToken ${accessToken}`
        }
    });
    if (response.status === 200) {
        await walter_goto('/');
    }

    return {
        fetch
    };
};
