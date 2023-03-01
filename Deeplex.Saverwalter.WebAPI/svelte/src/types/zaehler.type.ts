import type { AdresseEntry, AnhangEntry, SelectionEntry, ZaehlerstandEntry } from "$types";

export type ZaehlerEntry = {
    id: number;
    kennnummer: string;
    adresse: AdresseEntry;
    typ: string;
    allgemeinZaehler: SelectionEntry | undefined;
    wohnung: SelectionEntry | undefined;
    notiz: string;

    staende: ZaehlerstandEntry[];
    einzelzaehler: ZaehlerEntry[];
    anhaenge: AnhangEntry[];

}