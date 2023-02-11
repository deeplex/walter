import type { WohnungEntry } from "./wohnung.type";

export type UmlageEntry = {
    id: number;
    notiz: string;
    wohnungenBezeichnung: string;
    wohnungen: WohnungEntry[];
    typ: string;
}