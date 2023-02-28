import type { AnhangEntry } from "./anhang.type";
import type { BetriebskostenrechnungEntry } from "./betriebskostenrechnung.type";
import type { WohnungEntry } from "./wohnung.type";

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