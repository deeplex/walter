import type { AdresseEntry } from './adresse.type';
import type { AnhangEntry } from './anhang.type';
import type { VertragEntry } from './vertrag.type';
import type { WohnungEntry } from './wohnung.type';

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