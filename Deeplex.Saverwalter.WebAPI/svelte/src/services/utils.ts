// Canadian format allegedly is yyyy-mm-dd
export function convertDateCanadian(
    date: Date | undefined
): string | undefined {
    if (date && date.getFullYear) {
        const year = date.getFullYear();
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const day = date.getDate().toString().padStart(2, '0');
        return `${year}-${month}-${day}`;
    } else {
        return undefined;
    }
}

export function convertDateGerman(date: Date | undefined): string | undefined {
    if (date) {
        const year = date.getFullYear();
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const day = date.getDate().toString().padStart(2, '0');

        return `${day}.${month}.${year}`;
    } else {
        return undefined;
    }
}

export function convertTime(text: string | undefined): string | undefined {
    if (text) {
        return new Date(text).toLocaleString('de-DE');
    } else {
        return undefined;
    }
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

export function convertFixed2(value: number | undefined): string | undefined {
    return `${(value || 0).toFixed(2)}`;
}
