import type { WalterWohnungEntry } from '$walter/lib';
import type { WalterRechnungEntry } from '$walter/types';

export function convertToDataFromRechnungen(
    rechnungen: WalterRechnungEntry[]
): WalterDataType {
    return rechnungen.map((e) => ({
        group: e.typ,
        value: e.gesamtBetrag
    }));
}

export function convertToBeforeAfterDataFromRechnungen(
    rechnungen: WalterRechnungEntry[]
): WalterDataType {
    return rechnungen.map((e) => ({
        group: e.typ,
        value: [e.betragLetztesJahr, e.gesamtBetrag]
    }));
}

export function convertToDiffDataFromRechnungen(
    rechnungen: WalterRechnungEntry[]
) {
    return rechnungen.map((e) => ({
        group: e.typ,
        value: e.gesamtBetrag - e.betragLetztesJahr
    }));
}

export function convertToWFGruppe(wohnungen: WalterWohnungEntry[]) {
    return wohnungen.map((wohnung) => ({
        group: wohnung.bezeichnung,
        value: wohnung.wohnflaeche
    }));
}

export function convertToNFGruppe(wohnungen: WalterWohnungEntry[]) {
    return wohnungen.map((wohnung) => ({
        group: wohnung.bezeichnung,
        value: wohnung.nutzflaeche
    }));
}

export function convertToNEGruppe(wohnungen: WalterWohnungEntry[]) {
    return wohnungen.map((wohnung) => ({
        group: wohnung.bezeichnung,
        value: wohnung.einheiten
    }));
}

export type WalterDataType = {
    group: string;
    value: number | number[];
}[];
