import type { AdresseEntry, AnhangEntry, VertragEntry, WohnungEntry } from '$types';

export type PersonEntry = {
    id: number;
    guid: string;
    email: string;
    telefon: string;
    fax: string;
    mobil: string;
    notiz: string;
    name: string;

    natuerlichePerson: boolean;
    adresse: AdresseEntry;

    juristischePersonen: PersonEntry[];
    wohnungen: WohnungEntry[];
    vertraege: VertragEntry[];
    anhaenge: AnhangEntry[];
}