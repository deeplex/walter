export function dates(key: string) {
    switch (key) {
        case 'beginn':
        case 'ende':
        case 'datum':
        case 'betreffenderMonat':
        case 'zahlungsdatum':
            return true;
        default:
            return false;
    }
}

export function time(key: string) {
    switch (key) {
        case 'creationTime':
        case 'LastModified':
            return true;
        default:
            return false;
    }
}

export function formatToTableDate(date: string) {
    return new Date(date).toLocaleDateString('de-DE', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
}