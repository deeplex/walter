import type { BetriebskostenrechnungListEntry } from "./betriebskostenrechnunglist.type";
import type { WohnungListEntry } from "./wohnunglist.type";

export type UmlageEntry = {
    id: number;
    notiz: string;
    wohnungenBezeichnung: string;
    typ: string;

    betriebskostenrechnungen: BetriebskostenrechnungListEntry[];
    wohnungen: WohnungListEntry[];
}