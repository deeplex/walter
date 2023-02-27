import type { AdresseEntry } from './adresse.type';
import type { AnhangEntry } from './anhang.type';
import type { BetriebskostenrechnungEntry } from './betriebskostenrechnung.type';
import type { ErhaltungsaufwendungEntry } from './erhaltungsaufwendung.type';
import type { SelectionEntry } from './selection.type';
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