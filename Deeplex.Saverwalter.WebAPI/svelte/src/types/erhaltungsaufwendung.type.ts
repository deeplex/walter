import type { AnhangEntry } from "./anhang.type";
import type { PersonEntry } from "./person.type";
import type { WohnungEntry } from "./wohnung.type";

export type ErhaltungsaufwendungEntry = {
    id: number;
    betrag: number;
    datum: string;
    notiz: string;
    bezeichnung: string;
    wohnung: WohnungEntry;
    aussteller: PersonEntry;

    anhaenge: AnhangEntry[];
}