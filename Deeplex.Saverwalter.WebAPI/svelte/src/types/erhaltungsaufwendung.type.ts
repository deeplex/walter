import type { AnhangEntry, SelectionEntry } from "$types";

export type ErhaltungsaufwendungEntry = {
    id: number;
    betrag: number;
    datum: string;
    notiz: string;
    bezeichnung: string;
    wohnung: SelectionEntry;
    aussteller: SelectionEntry;

    anhaenge: AnhangEntry[];
}