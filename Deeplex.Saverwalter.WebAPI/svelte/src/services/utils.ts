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

export const walter_get = (url: string) => fetch(
    url,
    {
        method: 'GET',
        headers: {
            'Content-Type': 'text/json'
        }
    }
).then((e) => e.json());

export const walter_put = (url: string, body: any) => fetch(
    url,
    {
        method: 'PUT',
        headers: {
            'Content-Type': 'text/json'
        },
        body: JSON.stringify(body),
    }
).then((e) => e.json());