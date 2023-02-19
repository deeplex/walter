import type { AdresseEntry } from './adresse.type';
import type { AnhangListEntry } from './anhanglist.type';
import type { KontaktListEntry } from './kontaktlist.type';
import type { VertragListEntry } from './vertraglist.type';
import type { WohnungListEntry } from './wohnunglist.type';

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

    juristischePersonen: KontaktListEntry[];
    wohnungen: WohnungListEntry[];
    vertraege: VertragListEntry[];
    anhaenge: AnhangListEntry[];
}