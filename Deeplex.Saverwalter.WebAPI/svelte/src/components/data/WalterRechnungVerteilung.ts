import type { WalterRechnungEntry } from '$walter/types';

export function convertToData(rechnungen: WalterRechnungEntry[]) {
    return rechnungen.map((e) => ({
        group: e.typ,
        value: e.gesamtBetrag
    }));
}
