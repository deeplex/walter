import type { AdresseEntry, AnhangEntry, BetriebskostenrechnungEntry, ErhaltungsaufwendungEntry, SelectionEntry, UmlageEntry, VertragEntry, ZaehlerEntry } from '$types';

export type WohnungEntry = {
    adresse: AdresseEntry;
    id: number;
    bezeichnung: string;
    wohnflaeche: number;
    nutzflaeche: number;
    einheiten: number;
    notiz: string;
    anschrift: string;
    besitzer: SelectionEntry | undefined;

    haus: WohnungEntry[];
    zaehler: ZaehlerEntry[];
    vertraege: VertragEntry[];
    betriebskostenrechnungen: BetriebskostenrechnungEntry[];
    erhaltungsaufwendungen: ErhaltungsaufwendungEntry[];
    umlagen: UmlageEntry[];
    anhaenge: AnhangEntry[];

    bewohner: string;
}