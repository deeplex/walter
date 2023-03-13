import type { WalterAdresseEntry, WalterSelectionEntry, WalterVertragEntry, WalterWohnungEntry } from '$WalterTypes';

export type WalterPersonEntry = {
    id: number;
    guid: string;
    email: string;
    telefon: string;
    fax: string;
    mobil: string;
    notiz: string;
    name: string;
    selectedJuristischePersonen: WalterSelectionEntry[];

    natuerlichePerson: boolean;
    adresse: WalterAdresseEntry;

    juristischePersonen: WalterPersonEntry[];
    wohnungen: WalterWohnungEntry[];
    vertraege: WalterVertragEntry[];
};