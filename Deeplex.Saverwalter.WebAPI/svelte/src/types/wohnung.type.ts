import type { AdresseEntry } from './adresse.type';
import type { AnhangEntry } from './anhang.type';
import type { BetriebskostenrechnungEntry } from './betriebskostenrechnung.type';
import type { ErhaltungsaufwendungEntry } from './erhaltungsaufwendung.type';
import type { UmlageEntry } from './umlage.type';
import type { VertragEntry } from './vertrag.type';
import type { ZaehlerEntry } from './zaehler.type';

export type WohnungEntry = {
    adresse: AdresseEntry;
    id: number;
    bezeichnung: string;
    wohnflaeche: number;
    nutzflaeche: number;
    einheiten: number;
    notiz: string;
    anschrift: string;
    besitzerId: string;

    haus: WohnungEntry[];
    zaehler: ZaehlerEntry[];
    vertraege: VertragEntry[];
    betriebskostenrechnungen: BetriebskostenrechnungEntry[];
    erhaltungsaufwendungen: ErhaltungsaufwendungEntry[];
    umlagen: UmlageEntry[];
    anhaenge: AnhangEntry[];

    besitzer: string;
    bewohner: string;
}