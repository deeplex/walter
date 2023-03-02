import type { WalterBetriebskostenrechnungEntry, WalterErhaltungsaufwendungEntry, WalterPersonEntry, WalterUmlageEntry, WalterVertragEntry, WalterWohnungEntry, WalterZaehlerEntry } from "$WalterTypes";

export type WalterAnhangEntry = {
    id: string;
    fileName: string;
    creationTime: string;
    // notiz: string;

    betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[];
    erhaltungsaufwendungen: WalterErhaltungsaufwendungEntry[];
    natuerlichePersonen: WalterPersonEntry[];
    juristischePersonen: WalterPersonEntry[];
    umlagen: WalterUmlageEntry[];
    vertraege: WalterVertragEntry[];
    wohnungen: WalterWohnungEntry[];
    zaehler: WalterZaehlerEntry[];
}