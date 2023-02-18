import type { WohnungEntry } from "./wohnung.type";

export type VertragEntry = {
    id: number;
    beginn: string;
    ende: string | undefined;
    wohnung: WohnungEntry;
    ansprechpartnerId: string;
}