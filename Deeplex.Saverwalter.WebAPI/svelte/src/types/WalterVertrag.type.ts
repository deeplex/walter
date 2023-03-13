import type {
    WalterMieteEntry,
    WalterMietminderungEntry,
    WalterPersonEntry,
    WalterSelectionEntry,
    WalterVertragVersionEntry
} from "$WalterTypes";

export type WalterVertragEntry = {
    id: number;
    beginn: string;
    ende: string | undefined;
    wohnung: WalterSelectionEntry;
    ansprechpartner: WalterSelectionEntry | undefined;
    selectedMieter: WalterSelectionEntry[];
    notiz: string;

    mieterauflistung: string;

    versionen: WalterVertragVersionEntry[];
    mieter: WalterPersonEntry[];
    mieten: WalterMieteEntry[];
    mietminderungen: WalterMietminderungEntry[];
};