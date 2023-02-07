export class Adresse {
    strasse: string;
    hausnummer: string;
    postleitzahl: string;
    stadt: string;

    constructor(a: Adresse) {
        this.strasse = a?.strasse || "";
        this.hausnummer = a?.hausnummer || "";
        this.postleitzahl = a?.postleitzahl || "";
        this.stadt = a?.stadt || "";
    }
}