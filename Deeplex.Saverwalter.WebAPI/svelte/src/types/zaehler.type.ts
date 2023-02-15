import type { AdresseEntry } from "./adresse.type";

export type ZaehlerEntry = {
    id: number;
    kennnummer: string;
    allgemeinZaehler: ZaehlerEntry;
    adresse: AdresseEntry;
    typ: string;
}