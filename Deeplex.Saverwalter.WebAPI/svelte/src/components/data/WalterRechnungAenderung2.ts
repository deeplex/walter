import type { WalterRechnungEntry } from '$walter/types';

export function convertToData(rechnungen: WalterRechnungEntry[]) {
    return rechnungen.map((e) => ({
        ...e,
        group: e.typ,
        value: e.gesamtBetrag - e.betragLetztesJahr
    }));
}
