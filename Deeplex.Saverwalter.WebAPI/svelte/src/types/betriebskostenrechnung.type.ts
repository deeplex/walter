import type { UmlageEntry } from "./umlage.type";

export type BetriebskostenrechnungEntry = {
    id: number;
    betrag: number;
    betreffendesJahr: number;
    datum: string;
    notiz: string;
    umlage: UmlageEntry;
}
