import type { WalterVertragVersionEntry } from '$WalterLib';
import { convertDateCanadian } from '$WalterServices/utils';

export function getMietminderungEntry(vertragId: string) {
    const today = new Date();
    return {
        vertrag: { id: vertragId, text: '' },
        beginn: convertDateCanadian(today),
        ende: convertDateCanadian(
            new Date(today.setMonth(today.getMonth() + 1))
        )
    };
}

export function getVertragversionEntry(
    vertragId: string,
    lastVersion: WalterVertragVersionEntry | undefined
) {
    return {
        vertrag: { id: vertragId, text: '' },
        beginn: convertDateCanadian(new Date()),
        personenzahl: lastVersion?.personenzahl,
        grundmiete: lastVersion?.grundmiete
    };
}

export function getMieteEntry(
    vertragId: string,
    lastVersion: WalterVertragVersionEntry | undefined
) {
    return {
        vertrag: { id: vertragId, text: '' },
        zahlungsdatum: convertDateCanadian(new Date()),
        betrag: lastVersion?.grundmiete || 0
    };
}
