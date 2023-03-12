import { addToast } from "$WalterStore";
import type { WalterSelectionEntry } from "$WalterTypes";

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
    'Content-Type': 'text/json'
};

// =================================== GET ====================================

// Get fetch from svelte:
type fetchType = (input: RequestInfo | URL, init?: RequestInit | undefined) => Promise<Response>
export const walter_get = (url: string, f: fetchType): Promise<any> =>
    f(url, { method: 'GET', headers }).then(e => e.json());

// =================================== PUT ===================================

export const walter_put = (url: string, body: any) => fetch(
    url,
    { method: 'PUT', headers, body: JSON.stringify(body) }
).then(finishPut);

async function finishPut(e: Response) {
    const ok = e.status === 200;
    const kind = ok ? "success" : "error";
    const title = ok ? "Speichern Erfolgreich" : "Fehler";
    const j = await e.json();

    const subtitle = "TODO parse response body." // JSON.stringify(j);

    addToast({ title, kind, subtitle });
    return j;
}

// =================================== POST ====================================

export const walter_post = (url: string, body: any) => fetch(
    url,
    { method: 'POST', headers, body: JSON.stringify(body) }
).then(finishPost);

async function finishPost(e: Response) {
    const ok = e.status === 200;
    const kind = ok ? "success" : "error";
    const title = ok ? "Speichern erfolgreich" : "Fehler";
    const j = await e.json();

    const subtitle = "TODO parse response body." // JSON.stringify(j);

    addToast({ title, kind, subtitle });
    return j;
}

// =================================== DELETE =================================

export function walter_delete(url: string) {
    return fetch(url, { method: 'DELETE', headers }).then((e) => finishDelete(e));
}

function finishDelete(e: Response) {
    const ok = e.status === 200 || e.status === 204;
    const kind = ok ? "success" : "error";
    const title = ok ? "Löschen Erfolgreich" : "Fehler";
    // const j = await e.json();

    const subtitle = "TODO parse response body." // JSON.stringify(j);

    addToast({ title, kind, subtitle });
    return e;
}
