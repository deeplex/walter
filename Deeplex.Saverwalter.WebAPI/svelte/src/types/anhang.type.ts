import type { BetriebskostenrechnungListEntry } from "./betriebskostenrechnunglist.type";
import type { ErhaltungsaufwendungListEntry } from "./erhaltungsaufwendunglist.type";
import type { KontaktListEntry } from "./kontaktlist.type";
import type { UmlageListEntry } from "./umlagelist.type";
import type { VertragListEntry } from "./vertraglist.type";
import type { WohnungListEntry } from "./wohnunglist.type";
import type { ZaehlerListEntry } from "./zaehlerlist.type";

export type AnhangEntry = {
    id: string;
    fileName: string;
    creationTime: string;
    // notiz: string;

    betriebskostenrechnungen: BetriebskostenrechnungListEntry[];
    erhaltungsaufwendungen: ErhaltungsaufwendungListEntry[];
    natuerlichePersonen: KontaktListEntry[];
    juristischePersonen: KontaktListEntry[];
    umlagen: UmlageListEntry[];
    vertraege: VertragListEntry[];
    wohnungen: WohnungListEntry[];
    zaehler: ZaehlerListEntry[];
}