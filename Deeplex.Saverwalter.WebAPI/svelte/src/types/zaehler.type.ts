import type { AdresseEntry } from "./adresse.type";
import type { AnhangEntry } from "./anhang.type";
import type { SelectionEntry } from "./selection.type";
import type { ZaehlerstandEntry } from "./zaehlerstandlist.type";

export type ZaehlerEntry = {
    id: number;
    kennnummer: string;
    adresse: AdresseEntry;
    typ: string;
    allgemeinZaehler: SelectionEntry | undefined;
    wohnung: SelectionEntry | undefined;

    staende: ZaehlerstandEntry[];
    einzelzaehler: ZaehlerEntry[];
    anhaenge: AnhangEntry[];

}