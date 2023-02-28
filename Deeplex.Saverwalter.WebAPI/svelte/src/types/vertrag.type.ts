import type { AnhangEntry } from "./anhang.type";
import type { MieteEntry } from "./miete.type";
import type { MietminderungEntry } from "./mietminderung.type";
import type { PersonEntry } from "./person.type";
import type { SelectionEntry } from "./selection.type";

export type VertragEntry = {
    id: number;
    beginn: string;
    ende: string | undefined;
    wohnung: SelectionEntry;
    ansprechpartner: SelectionEntry | undefined;
    notiz: string;

    mieterauflistung: string;

    mieter: PersonEntry[];
    mieten: MieteEntry[];
    mietminderungen: MietminderungEntry[];
    anhaenge: AnhangEntry[];
}