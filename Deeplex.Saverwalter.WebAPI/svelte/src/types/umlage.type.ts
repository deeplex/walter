import type { AnhangEntry } from "./anhang.type";
import type { BetriebskostenrechnungEntry } from "./betriebskostenrechnung.type";
import type { WohnungEntry } from "./wohnung.type";

export type UmlageEntry = {
    id: number;
    notiz: string;
    wohnungenBezeichnung: string;
    typ: string;

    betriebskostenrechnungen: BetriebskostenrechnungEntry[];
    wohnungen: WohnungEntry[];

    anhaenge: AnhangEntry[];
}