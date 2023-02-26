import type { AnhangEntry } from "./anhang.type";
import type { MieteEntry } from "./miete.type";
import type { MietminderungEntry } from "./mietminderung.type";
import type { PersonEntry } from "./person.type";
import type { WohnungEntry } from "./wohnung.type";

export type VertragEntry = {
    id: number;
    beginn: string;
    ende: string | undefined;
    wohnung: WohnungEntry;
    ansprechpartnerId: string;

    mieter: PersonEntry[];
    mieten: MieteEntry[];
    mietminderungen: MietminderungEntry[];

    anhaenge: AnhangEntry[];

    mieterauflistung: string;
}