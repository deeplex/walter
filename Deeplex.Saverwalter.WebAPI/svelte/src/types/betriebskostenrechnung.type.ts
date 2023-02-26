import type { AnhangEntry } from "./anhang.type";
import type { UmlageEntry } from "./umlage.type";
import type { WohnungEntry } from "./wohnung.type";

export type BetriebskostenrechnungEntry = {
    id: number;
    betrag: number;
    betreffendesJahr: number;
    datum: string;
    notiz: string;
    umlage: UmlageEntry;

    wohnungen: WohnungEntry[];
    anhaenge: AnhangEntry[];
}
