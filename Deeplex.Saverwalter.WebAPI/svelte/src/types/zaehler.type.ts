import type { AdresseEntry } from "./adresse.type";
import type { AnhangListEntry } from "./anhanglist.type";
import type { ZaehlerListEntry } from "./zaehlerlist.type";
import type { ZaehlerstandListEntry } from "./zaehlerstandlist.type";

export type ZaehlerEntry = {
    id: number;
    kennnummer: string;
    adresse: AdresseEntry;
    typ: string;
    allgemeinZaehler: ZaehlerEntry;

    staende: ZaehlerstandListEntry[];
    einzelzaehler: ZaehlerListEntry[];
    anhaenge: AnhangListEntry[];
}