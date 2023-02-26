import type { AdresseEntry } from "./adresse.type";
import type { AnhangEntry } from "./anhang.type";
import type { ZaehlerstandEntry } from "./zaehlerstandlist.type";

export type ZaehlerEntry = {
    id: number;
    kennnummer: string;
    adresse: AdresseEntry;
    typ: string;
    allgemeinZaehler: ZaehlerEntry;

    staende: ZaehlerstandEntry[];
    einzelzaehler: ZaehlerEntry[];
    anhaenge: AnhangEntry[];

    wohnung: string;
}