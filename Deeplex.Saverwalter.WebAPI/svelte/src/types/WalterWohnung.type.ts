import type { WalterAdresseEntry, WalterAnhangEntry, WalterBetriebskostenrechnungEntry, WalterErhaltungsaufwendungEntry, WalterSelectionEntry, WalterUmlageEntry, WalterVertragEntry, WalterZaehlerEntry } from '$WalterTypes';

export type WalterWohnungEntry = {
    adresse: WalterAdresseEntry;
    id: number;
    bezeichnung: string;
    wohnflaeche: number;
    nutzflaeche: number;
    einheiten: number;
    notiz: string;
    anschrift: string;
    besitzer: WalterSelectionEntry | undefined;

    haus: WalterWohnungEntry[];
    zaehler: WalterZaehlerEntry[];
    vertraege: WalterVertragEntry[];
    betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[];
    erhaltungsaufwendungen: WalterErhaltungsaufwendungEntry[];
    umlagen: WalterUmlageEntry[];
    anhaenge: WalterAnhangEntry[];

    bewohner: string;
}