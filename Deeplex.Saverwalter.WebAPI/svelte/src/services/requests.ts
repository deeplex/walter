import { goto } from "$app/navigation";
import { addToast, openModal } from "$WalterStore";
import type { WalterSelectionEntry } from "$WalterTypes";

export const walter_selection = {
    betriebskostentypen(f: fetchType): Promise<WalterSelectionEntry> {
        return walter_get('/api/selection/betriebskostentypen', f)
    },
    umlagen(f: fetchType): Promise<WalterSelectionEntry> {
        return walter_get('/api/selection/umlagen', f)
    },
    kontakte(f: fetchType): Promise<WalterSelectionEntry> {
        return walter_get('/api/selection/kontakte', f)
    },
    wohnungen(f: fetchType): Promise<WalterSelectionEntry> {
        return walter_get('/api/selection/wohnungen', f)
    },
    umlageschluessel(f: fetchType): Promise<WalterSelectionEntry> {
        return walter_get('/api/selection/umlageschluessel', f)
    },
    zaehler(f: fetchType): Promise<WalterSelectionEntry> {
        return walter_get('/api/selection/zaehler', f)
    },
    zaehlertypen(f: fetchType): Promise<WalterSelectionEntry> {
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
    // TODO Navigate to target position.
).then(finishPost);

async function finishPost(e: Response) {
    const ok = e.status === 200;
    const kind = ok ? "success" : "error";
    const title = ok ? "Speichern Erfolgreich" : "Fehler";
    const j = await e.json();

    const subtitle = "TODO parse response body." // JSON.stringify(j);

    addToast({ title, kind, subtitle });
    return j;
}

// =================================== DELETE =================================

export const walter_delete = (url: string, entry_title: string, nav: string = "/") => {
    const content = `Bist du sicher, dass du ${entry_title} löschen möchtest?
    Dieser Vorgang kann nicht rückgängig gemacht werden.`

    openModal({
        modalHeading: "Löschen",
        content,
        danger: true,
        primaryButtonText: "Löschen",
        submit: () => really_delete_walter(url, nav),
    });
}

function really_delete_walter(url: string, nav: string) {
    return fetch(
        url,
        { method: 'DELETE', headers }
    ).then((e) => finishDelete(e, nav));
}

function finishDelete(e: Response, nav: string) {
    const ok = e.status === 200 || e.status === 204;
    const kind = ok ? "success" : "error";
    const title = ok ? "Löschen Erfolgreich" : "Fehler";
    // const j = await e.json();

    const subtitle = "TODO parse response body." // JSON.stringify(j);

    if (nav) {
        goto("/")
    }

    addToast({ title, kind, subtitle });
    return e;
}
