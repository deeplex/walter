import { walter_get } from "$WalterServices/requests";
import { toLocaleIsoString } from "$WalterServices/utils";
import type { WalterBetriebskostenabrechnungEntry, WalterVertragVersionEntry } from "$WalterTypes";

export function getMietminderungEntry(vertragId: string) {
    const today = new Date();
    return {
        vertrag: { id: vertragId, text: '' },
        beginn: toLocaleIsoString(today),
        ende: toLocaleIsoString(new Date(today.setMonth(today.getMonth() + 1)))
    }
};

export function getVertragversionEntry(vertragId: string, lastVersion: WalterVertragVersionEntry | undefined) {
    return {
        vertrag: { id: vertragId, text: '' },
        beginn: toLocaleIsoString(new Date()),
        personenzahl: lastVersion?.personenzahl,
        grundmiete: lastVersion?.grundmiete
    }
};

export function getMieteEntry(vertragId: string, lastVersion: WalterVertragVersionEntry | undefined) {
    return {
        vertrag: { id: vertragId, text: '' },
        zahlungsdatum: toLocaleIsoString(new Date()),
        betrag: lastVersion?.grundmiete || 0
    };
}

type fetchType = (input: RequestInfo | URL, init?: RequestInit | undefined) => Promise<Response>
export function loadAbrechnung(vertragId: string, year: string, fetch: fetchType) {
    const abrechnungURL = `/api/betriebskostenabrechnung/${vertragId}/${year}`;

    return walter_get(abrechnungURL, fetch) as Promise<WalterBetriebskostenabrechnungEntry>;
}