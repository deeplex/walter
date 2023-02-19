import type { AnhangListEntry } from "./anhanglist.type";
import type { UmlageEntry } from "./umlage.type";
import type { WohnungListEntry } from "./wohnunglist.type";

export type BetriebskostenrechnungEntry = {
    id: number;
    betrag: number;
    betreffendesJahr: number;
    datum: string;
    notiz: string;
    umlage: UmlageEntry;

    wohnungen: WohnungListEntry[];
    anhaenge: AnhangListEntry[];
}
