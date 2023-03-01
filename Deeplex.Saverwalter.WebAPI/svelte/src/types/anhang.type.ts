import type { BetriebskostenrechnungEntry, ErhaltungsaufwendungEntry, PersonEntry, UmlageEntry, VertragEntry, WohnungEntry, ZaehlerEntry } from "$types";

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