import type { AnhangEntry, BetriebskostenrechnungEntry, WohnungEntry } from "$types";

export type UmlageEntry = {
    id: number;
    notiz: string;
    beschreibung: string;
    wohnungenBezeichnung: string;
    // TODO => SelectionEntry
    zaehler: string;
    // TODO => SelectionEntry
    typ: string;
    // TODO => SelectionEntry
    schluessel: string;

    betriebskostenrechnungen: BetriebskostenrechnungEntry[];
    wohnungen: WohnungEntry[];
    anhaenge: AnhangEntry[];
}