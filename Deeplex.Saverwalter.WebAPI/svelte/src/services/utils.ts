import { addToast } from '../store';

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
async function finishRequest(e: Response) {
    const ok = e.status === 200;
    const kind = ok ? "success" : "error";
    const title = ok ? "Speichern Erfolgreich" : "Fehler";
    const j = await e.json();

    const subtitle = "TODO parse response body." // JSON.stringify(j);

    addToast({ title, kind, subtitle });
    return j;
}

const headers = {
    'Content-Type': 'text/json'
};

export const walter_get = (url: string) => fetch(
    url, { method: 'GET', headers }
).then(e => e.json());

export const walter_put = (url: string, body: any) => fetch(
    url,
    { method: 'PUT', headers, body: JSON.stringify(body) }
).then(finishRequest);
