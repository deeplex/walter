import type { AnhangEntry, MieteEntry, MietminderungEntry, PersonEntry, SelectionEntry } from "$types";

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