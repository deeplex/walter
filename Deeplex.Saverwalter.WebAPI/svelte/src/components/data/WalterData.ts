import type {
    WalterBetriebskostenrechnungEntry,
    WalterWohnungEntry
} from '$walter/lib';
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
    return rechnungen.map((rechnung) => ({
        group: rechnung.typ,
        value: [rechnung.betragLetztesJahr, rechnung.gesamtBetrag]
    }));
}

export function convertToDiffDataFromRechnungen(
    rechnungen: WalterRechnungEntry[]
): WalterDataType {
    return rechnungen.map((rechnung) => ({
        group: rechnung.typ,
        value: rechnung.gesamtBetrag - rechnung.betragLetztesJahr
    }));
}

export function convertToRechnungenFromUmlage(
    rechnungen: WalterBetriebskostenrechnungEntry[]
): WalterDataType {
    return rechnungen.map((rechnung) => ({
        group: `${rechnung.umlage.text}`,
        key: `${rechnung.betreffendesJahr}`,
        value: rechnung.betrag
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
    key?: string;
    date?: string; // Date in Canadian Format
}[];
