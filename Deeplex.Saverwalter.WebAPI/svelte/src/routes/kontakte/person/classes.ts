import { Adresse } from "../../../components/adresse";
import type { JuristischePersonEntry } from "../jur/[slug]/classes";
import type { NatuerlichePersonEntry } from "../nat/[slug]/classes";

export class PersonEntry {
    id: string;
    guid: string;
    email: string;
    telefon: string;
    fax: string;
    mobil: string;
    notiz: string;
    name: string;

    natuerlichePerson: boolean;
    adresse: Adresse;

    constructor(e: NatuerlichePersonEntry | JuristischePersonEntry) {
        this.guid = e.guid;
        this.id = this.guid;
        this.name = e.name;
        this.email = e.email;
        this.telefon = e.telefon;
        this.fax = e.fax;
        this.mobil = e.mobil;
        this.notiz = e.notiz;

        this.natuerlichePerson = e.natuerlichePerson;
        this.adresse = new Adresse(e.adresse);
    }
}