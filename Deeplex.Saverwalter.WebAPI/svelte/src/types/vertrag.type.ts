import type { AnhangListEntry } from "./anhanglist.type";
import type { KontaktListEntry } from "./kontaktlist.type";
import type { MieteListEntry } from "./mietelist.type";
import type { MietminderungListEntry } from "./mietminderunglist.type";
import type { WohnungEntry } from "./wohnung.type";

export type VertragEntry = {
    id: number;
    beginn: string;
    ende: string | undefined;
    wohnung: WohnungEntry;
    ansprechpartnerId: string;

    mieter: KontaktListEntry[];
    mieten: MieteListEntry[];
    mietminderungen: MietminderungListEntry[];

    anhaenge: AnhangListEntry[];
}