import type { AdresseEntry } from './adresse.type';

export type WohnungEntry = {
    adresse: AdresseEntry;
    id: number;
    bezeichnung: string;
    wohnflaeche: number;
    nutzflaeche: number;
    einheiten: number;
    notiz: string;
    anschrift: string
}