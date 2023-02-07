import type { Adresse } from "../../../components/adresse";

export class WohnungEntry {
    adresse: Adresse;
    bezeichnung: string;
    wohnflaeche: number;
    nutzflaeche: number;
    einheiten: number;
    notiz: string;

    constructor(e: WohnungEntry) {
        this.adresse = e.adresse;
        this.bezeichnung = e.bezeichnung;
        this.wohnflaeche = e.wohnflaeche;
        this.nutzflaeche = e.nutzflaeche;
        this.einheiten = e.einheiten;
        this.notiz = e.notiz;
    }
}