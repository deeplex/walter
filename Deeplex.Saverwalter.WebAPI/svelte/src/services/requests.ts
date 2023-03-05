import { goto } from "$app/navigation";
import { addToast, openModal } from "$WalterStore";

const headers = {
    'Content-Type': 'text/json'
};

// =================================== GET ====================================

export const walter_get = (url: string) => fetch(
    url, { method: 'GET', headers }
).then(e => e.json());

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

export const walter_delete = (url: string, entry_title: string) => {
    const content = `Bist du sicher, dass du ${entry_title} löschen möchtest?
    Dieser Vorgang kann nicht rückgängig gemacht werden.`

    openModal({
        modalHeading: "Eintrag löschen",
        content,
        danger: true,
        primaryButtonText: "Löschen",
        submit: () => really_delete_walter(url),
    });
}

function really_delete_walter(url: string) {
    return fetch(
        url,
        { method: 'DELETE', headers }
    ).then(finishDelete);
}

function finishDelete(e: Response) {
    const ok = e.status === 200;
    const kind = ok ? "success" : "error";
    const title = ok ? "Löschen Erfolgreich" : "Fehler";
    // const j = await e.json();

    const subtitle = "TODO parse response body." // JSON.stringify(j);

    goto("/")

    addToast({ title, kind, subtitle });
    return e;
}
