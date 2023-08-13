export function dates(key: string) {
    switch (key) {
        case 'beginn':
        case 'ende':
        case 'datum':
        case 'betreffenderMonat':
        case 'zahlungsdatum':
        case 'lastZaehlerstand.datum':
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

export function euro(key: string) {
    switch (key) {
        case 'betrag':
        case 'grundmiete':
        case 'kosten':
        case 'gesamtBetrag':
        case 'betragLetztesJahr':
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
