import { PersonEntry } from "../../person/classes";

export class NatuerlichePersonEntry extends PersonEntry {
    vorname: string;
    nachname: string;

    constructor(e: NatuerlichePersonEntry) {
        super(e);
        this.vorname = e.vorname;
        this.nachname = e.nachname;
    }
}