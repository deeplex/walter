export class BetriebskostenrechnungEntry {
    id: number;
    betrag: number;
    betreffendesjahr: number;
    datum: string;
    notiz: string;
    umlage: string;

    constructor(e: BetriebskostenrechnungEntry) {
        this.id = e.id;
        this.betrag = e.betrag;
        this.betreffendesjahr = e.betreffendesjahr;
        this.datum = e.datum;
        this.notiz = e.notiz;
        this.umlage = e.umlage;
    }
}
