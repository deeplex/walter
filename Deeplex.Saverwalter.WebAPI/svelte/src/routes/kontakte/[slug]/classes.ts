class PersonEntry {
    id: string;
    guid: string;
    email: string;
    telefon: string;
    natuerlichePerson: boolean;

    constructor(e: NatuerlichePersonEntry | JuristischePersonEntry) {
        this.guid = e.guid;
        this.id = this.guid;
        this.email = e.email;
        this.telefon = e.telefon;
        this.natuerlichePerson = e.natuerlichePerson;
    }
}

export class JuristischePersonEntry extends PersonEntry {
    name: string;

    constructor(e: JuristischePersonEntry) {
        super(e);
        this.name = e.name;

    }
}

export class NatuerlichePersonEntry extends PersonEntry {
    vorname: string;
    nachname: string;
    name: string;

    constructor(e: NatuerlichePersonEntry) {
        super(e);
        this.name = e.name;
        this.vorname = e.vorname;
        this.nachname = e.nachname;
    }
}