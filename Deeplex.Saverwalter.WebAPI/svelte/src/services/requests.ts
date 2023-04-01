import type { WalterSelectionEntry, WalterToastContent } from "$WalterLib";
import { addToast } from "$WalterStore";
import { goto } from "$app/navigation";

export const walter_selection = {
    betriebskostentypen(f: fetchType): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/betriebskostentypen', f)
    },
    umlagen(f: fetchType): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/umlagen', f)
    },
    kontakte(f: fetchType): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/kontakte', f)
    },
    juristischePersonen(f: fetchType): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/juristischepersonen', f)
    },
    wohnungen(f: fetchType): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/wohnungen', f)
    },
    umlageschluessel(f: fetchType): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/umlageschluessel', f)
    },
    zaehler(f: fetchType): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/zaehler', f)
    },
    zaehlertypen(f: fetchType): Promise<WalterSelectionEntry[]> {
        return walter_get('/api/selection/zaehlertypen', f)
    },
}

const headers = {
    'Content-Type': 'text/json',
};

function handleUnauthorized(e: Response) {
    if (e.status === 401) {
        goto("/login");
        throw new Error("Unauthorized access. Redirecting to login page.");
    }
    return e;
}

// =================================== GET ====================================

// Get fetch from svelte:
type fetchType = (input: RequestInfo | URL, init?: RequestInit | undefined) => Promise<Response>
export const walter_get = (url: string, fetch: fetchType): Promise<any> =>
    fetch(url, { method: 'GET', headers })
        .then(handleUnauthorized)
        .then(e => e.json());

// =================================== PUT ===================================

export const walter_put = (url: string, body: any, toast?: WalterToastContent) =>
    fetch(url, { method: 'PUT', headers, body: JSON.stringify(body) })
        .then(handleUnauthorized)
        .then(e => finishPut(e, toast));

async function finishPut(e: Response, toast?: WalterToastContent) {
    const j = await e.json();

    toast && addToast(toast, e.status === 200, j);

    return j;
}

// =================================== POST ====================================

export const walter_post = (url: string, body: any, toast?: WalterToastContent) =>
    fetch(url, { method: 'POST', headers, body: JSON.stringify(body) })
        .then(handleUnauthorized)
        .then(e => finishPost(e, toast));

async function finishPost(e: Response, toast?: WalterToastContent) {
    const j = await e.json();

    toast && addToast(toast, e.status === 200, j);

    return j;
}

// =================================== DELETE =================================

export const walter_delete = (url: string, toast?: WalterToastContent) =>
    fetch(url, { method: 'DELETE', headers })
        .then(handleUnauthorized)
        .then((e) => finishDelete(e, toast));

function finishDelete(e: Response, toast?: WalterToastContent) {
    toast && addToast(toast, e.status === 200);
    return e;
}
