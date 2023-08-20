import type { WalterSelectionEntry, WalterToastContent } from '$walter/lib';
import { addToast } from '$walter/store';
import { getAccessToken } from './auth';
import { walter_goto } from './utils';

export const walter_selection = {
    adressen(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/adressen', fetchImpl);
    },
    anreden(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/anreden', fetchImpl);
    },
    betriebskostenrechnungen(
        fetchImpl: typeof fetch
    ): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/betriebskostenrechnungen', fetchImpl);
    },
    erhaltungsaufwendungen(
        fetchImpl: typeof fetch
    ): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/erhaltungsaufwendungen', fetchImpl);
    },
    natuerlichePersonen(
        fetchImpl: typeof fetch
    ): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/natuerlichepersonen', fetchImpl);
    },
    juristischePersonen(
        fetchImpl: typeof fetch
    ): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/juristischepersonen', fetchImpl);
    },
    mieten(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/mieten', fetchImpl);
    },
    mietminderungen(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/mietminderungen', fetchImpl);
    },
    umlagen(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/umlagen', fetchImpl);
    },
    vertraege(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/vertraege', fetchImpl);
    },
    // vertragversionen(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
    //     return walter_get('/api/selection/vertragversionen', fetchImpl);
    // },
    zaehler(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/zaehler', fetchImpl);
    },
    zaehlerstaende(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/zaehlerstaende', fetchImpl);
    },
    wohnungen(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/wohnungen', fetchImpl);
    },
    betriebskostentypen(
        fetchImpl: typeof fetch
    ): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/betriebskostentypen', fetchImpl);
    },
    kontakte(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/kontakte', fetchImpl);
    },
    umlageschluessel(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/umlageschluessel', fetchImpl);
    },
    umlagen_verbrauch(
        fetchImpl: typeof fetch
    ): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/umlagen_verbrauch', fetchImpl);
    },
    umlagen_wohnungen(
        fetchImpl: typeof fetch
    ): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/umlagen_wohnungen', fetchImpl);
    },
    zaehlertypen(fetchImpl: typeof fetch): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/zaehlertypen', fetchImpl);
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
        await walter_goto('/login');
        throw new Error('Unauthorized access. Redirecting to login page.');
    }
    return response;
}

// =================================== GET ====================================

export const walter_get = (
    url: string,
    fetchImpl: typeof fetch
): Promise<any> =>
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
