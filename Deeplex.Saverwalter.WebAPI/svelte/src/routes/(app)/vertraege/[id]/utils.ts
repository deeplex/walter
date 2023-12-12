import type {
    WalterVertragEntry,
    WalterVertragVersionEntry
} from '$walter/lib';
import { convertDateCanadian } from '$walter/services/utils';

export function getMietminderungEntry(vertrag: WalterVertragEntry) {
    const today = new Date();
    return {
        vertrag: { id: vertrag.id, text: '' },
        beginn: convertDateCanadian(today),
        ende: convertDateCanadian(
            new Date(today.setMonth(today.getMonth() + 1))
        ),
        permissions: vertrag.permissions
    };
}

export function getVertragversionEntry(vertrag: WalterVertragEntry) {
    return {
        vertrag: { id: vertrag.id, text: '' },
        beginn: convertDateCanadian(new Date()),
        personenzahl:
            vertrag.versionen[vertrag.versionen.length - 1]?.personenzahl,
        grundmiete: vertrag.versionen[vertrag.versionen.length - 1]?.grundmiete,
        permissions: vertrag.permissions
    };
}

export function getMieteEntry(vertrag: WalterVertragEntry) {
    return {
        vertrag: { id: vertrag.id, text: '' },
        zahlungsdatum: convertDateCanadian(new Date()),
        betrag:
            vertrag.versionen[vertrag.versionen.length - 1]?.grundmiete || 0,
        permissions: vertrag.permissions
    };
}
