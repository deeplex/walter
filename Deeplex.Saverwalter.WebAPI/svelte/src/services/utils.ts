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

export function toLocaleIsoString(date: Date) {
    var locale = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    return locale.toISOString();
}

export function convertEuro(value: number | undefined): string | undefined {
    if (value) {
        return `${value.toFixed(2)}€`;
    }
    else {
        return undefined;
    }
}