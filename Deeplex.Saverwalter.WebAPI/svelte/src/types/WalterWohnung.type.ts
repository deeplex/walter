import type {
    WalterAdresseEntry,
    WalterBetriebskostenrechnungEntry,
    WalterErhaltungsaufwendungEntry,
    WalterSelectionEntry,
    WalterUmlageEntry,
    WalterVertragEntry,
    WalterZaehlerEntry
} from '$WalterTypes';

export type WalterWohnungEntry = {
    adresse: WalterAdresseEntry;
    id: number;
    bezeichnung: string;
    wohnflaeche: number;
    nutzflaeche: number;
    einheiten: number;
    notiz: string;
    besitzer: WalterSelectionEntry | undefined;

    haus: WalterWohnungEntry[];
    zaehler: WalterZaehlerEntry[];
    vertraege: WalterVertragEntry[];
    betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[];
    erhaltungsaufwendungen: WalterErhaltungsaufwendungEntry[];
    umlagen: WalterUmlageEntry[];

    bewohner: string;
};