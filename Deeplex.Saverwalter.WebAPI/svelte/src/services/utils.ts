import { afterNavigate } from '$app/navigation';
import { base } from '$app/paths';
import { addToast, openModal } from '$store';

export function convertDate(text: string | undefined): string | undefined {
    if (text) {
        return new Date(text).toLocaleDateString("de-DE");
    }
    else {
        return undefined;
    }
}

export function convertTime(text: string | undefined): string | undefined {
    if (text) {
        return new Date(text).toLocaleString("de-DE");
    }
    else {
        return undefined;
    }
}

export function convertEuro(value: number | undefined): string | undefined {
    if (value) {
        return `${value.toFixed(2)}€`;
    }
    else {
        return undefined;
    }
}

const headers = {
    'Content-Type': 'text/json'
};

// =================================== GET ====================================

export const walter_get = (url: string) => fetch(
    url, { method: 'GET', headers }
).then(e => e.json());

// ================================ POST / PUT ================================

export const walter_put = (url: string, body: any) => fetch(
    url,
    { method: 'PUT', headers, body: JSON.stringify(body) }
).then(finishPutPost);

async function finishPutPost(e: Response) {
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

async function finishDelete(e: Response) {
    const ok = e.status === 200;
    const kind = ok ? "success" : "error";
    const title = ok ? "Löschen Erfolgreich" : "Fehler";
    const j = await e.json();

    const subtitle = "TODO parse response body." // JSON.stringify(j);

    let previousPage: string = base;

    afterNavigate(({ from }) => {
        previousPage = from?.url.pathname || previousPage
    })

    addToast({ title, kind, subtitle });
    return j;
}
