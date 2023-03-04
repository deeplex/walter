import type { WalterAnhangEntry, WalterMieteEntry, WalterMietminderungEntry, WalterPersonEntry, WalterSelectionEntry } from "$WalterTypes";

export type WalterVertragEntry = {
    id: number;
    beginn: string;
    ende: string | undefined;
    wohnung: WalterSelectionEntry;
    ansprechpartner: WalterSelectionEntry | undefined;
    selectedMieter: WalterSelectionEntry[];
    notiz: string;

    mieterauflistung: string;

    mieter: WalterPersonEntry[];
    mieten: WalterMieteEntry[];
    mietminderungen: WalterMietminderungEntry[];
    anhaenge: WalterAnhangEntry[];
}