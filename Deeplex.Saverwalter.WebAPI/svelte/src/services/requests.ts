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

import type { WalterSelectionEntry, WalterToastContent } from '$walter/lib';
import { addToast, changeTracker } from '$walter/store';
import { getAccessToken } from './auth';
import { walter_goto } from './utils';

export const walter_selection = {
    adressen(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/adressen', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    anreden(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/anreden', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    betriebskostenrechnungen(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/betriebskostenrechnungen',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    erhaltungsaufwendungen(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/erhaltungsaufwendungen',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    hkvo_p9a2(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/hkvo_p9a2', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    juristischePersonen(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/juristischepersonen',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    mieten(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/mieten', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    mietminderungen(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/mietminderungen',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    rechtsformen(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/rechtsformen', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    umlagen(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/umlagen', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    umlagetypen(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/umlagetypen', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    vertraege(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/vertraege', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    // vertragversionen(fetchImpl: typeof fetch) {
    //     return walter_get(
    //         '/api/selection/vertragversionen',
    //         fetchImpl
    //     ) as Promise<WalterSelectionEntry[]>;
    // },
    zaehler(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/zaehler', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    zaehlerstaende(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/zaehlerstaende',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    wohnungen(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/wohnungen', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    kontakte(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/kontakte', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    umlageschluessel(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/umlageschluessel',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    umlagen_verbrauch(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/umlagen_verbrauch',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    umlagen_wohnungen(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/umlagen_wohnungen',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    zaehlertypen(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/zaehlertypen', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    },
    verwalterrollen(fetchImpl: typeof fetch) {
        return walter_get(
            '/api/selection/verwalterrollen',
            fetchImpl
        ) as Promise<WalterSelectionEntry[]>;
    },
    userrole(fetchImpl: typeof fetch) {
        return walter_get('/api/selection/userrole', fetchImpl) as Promise<
            WalterSelectionEntry[]
        >;
    }
};

export async function walter_fetch(
    fetchImpl: typeof fetch,
    url: string,
    init: RequestInit = {}
): Promise<Response> {
    const headers = new Headers(init.headers);
    headers.set('Content-Type', 'application/json');
    const accessToken = getAccessToken();
    if (accessToken != null) {
        headers.set('Authorization', `X-WalterToken ${accessToken}`);
    }

    init.headers = headers;
    const response = await fetchImpl(url, init);
    if (response.status === 401) {
        changeTracker.set(0);
        await walter_goto('/login');
        throw new Error('Unauthorized access. Redirecting to login page.');
    }
    return response;
}

// =================================== GET ====================================

export const walter_get = (
    url: string,
    fetchImpl: typeof fetch
): Promise<unknown> =>
    walter_fetch(fetchImpl, url, { method: 'GET' }).then((e) => e.json());

// =================================== PUT ===================================

export const walter_put = (
    url: string,
    body: unknown,
    toast?: WalterToastContent
) =>
    walter_fetch(fetch, url, {
        method: 'PUT',
        body: JSON.stringify(body)
    }).then((e) => finishPut(e, toast));

async function finishPut(e: Response, toast?: WalterToastContent) {
    const j = await e.json();

    toast && addToast(toast, e.status === 200, j);

    return e;
}

// =================================== POST ====================================

export async function walter_post(
    url: string,
    body: unknown
): Promise<Response> {
    const response = await walter_fetch(fetch, url, {
        method: 'POST',
        body: JSON.stringify(body)
    });

    return response;
}

// =================================== DELETE =================================

export const walter_delete = (url: string, toast?: WalterToastContent) =>
    walter_fetch(fetch, url, { method: 'DELETE' }).then((e) =>
        finishDelete(e, toast)
    );

function finishDelete(e: Response, toast?: WalterToastContent) {
    toast && addToast(toast, e.status === 200);
    return e;
}
