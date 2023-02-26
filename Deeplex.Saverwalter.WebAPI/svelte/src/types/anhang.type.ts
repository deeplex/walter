import type { BetriebskostenrechnungEntry } from "./betriebskostenrechnung.type";
import type { ErhaltungsaufwendungEntry } from "./erhaltungsaufwendung.type";
import type { PersonEntry } from "./person.type";
import type { UmlageEntry } from "./umlage.type";
import type { VertragEntry } from "./vertrag.type";
import type { WohnungEntry } from "./wohnung.type";
import type { ZaehlerEntry } from "./zaehler.type";

export type AnhangEntry = {
    id: string;
    fileName: string;
    creationTime: string;
    // notiz: string;

    betriebskostenrechnungen: BetriebskostenrechnungEntry[];
    erhaltungsaufwendungen: ErhaltungsaufwendungEntry[];
    natuerlichePersonen: PersonEntry[];
    juristischePersonen: PersonEntry[];
    umlagen: UmlageEntry[];
    vertraege: VertragEntry[];
    wohnungen: WohnungEntry[];
    zaehler: ZaehlerEntry[];
}