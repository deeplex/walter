import type { AdresseEntry } from './adresse.type';
import type { BetriebskostenrechnungListEntry } from './betriebskostenrechnunglist.type';
import type { ErhaltungsaufwendungListEntry } from './erhaltungsaufwendunglist.type';
import type { UmlageListEntry } from './umlagelist.type';
import type { VertragListEntry } from './vertraglist.type';
import type { WohnungListEntry } from './wohnunglist.type';
import type { ZaehlerListEntry } from './zaehlerlist.type';

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

    haus: WohnungListEntry[];
    zaehler: ZaehlerListEntry[];
    vertraege: VertragListEntry[];
    betriebskostenrechnungen: BetriebskostenrechnungListEntry[];
    erhaltungsaufwendungen: ErhaltungsaufwendungListEntry[];
    umlagen: UmlageListEntry[];
}