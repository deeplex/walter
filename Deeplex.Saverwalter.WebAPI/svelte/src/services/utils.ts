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

export const request_options = {
    method: 'GET',
    headers: {
        'Content-Type': 'text/json'
    }
};

export const walter_get = (url: string) => fetch(
    url,
    request_options
).then((e) => e.json());