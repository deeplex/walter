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
    return `${(value || 0).toFixed(2)} €`;
}

export function convertPercent(value: number | undefined): string | undefined {
    return `${((value || 0) * 100).toFixed(2)}%`;
}

export function convertM2(value: number | undefined): string | undefined {
    return `${(value || 0).toFixed(2)} m²`;
}