import type { AdresseEntry } from './adresse.type';

export type PersonEntry = {
    id: string;
    guid: string;
    email: string;
    telefon: string;
    fax: string;
    mobil: string;
    notiz: string;
    name: string;

    natuerlichePerson: boolean;
    adresse: AdresseEntry;
}