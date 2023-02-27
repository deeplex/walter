import type { AnhangEntry } from "./anhang.type";
import type { SelectionEntry } from "./selection.type";

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