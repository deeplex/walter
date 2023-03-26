import { WalterSelectionEntry } from "$WalterLib";
import { WalterApiHandler } from "./WalterApiHandler";

export class WalterErhaltungsaufwendungEntry extends WalterApiHandler {
    public static ApiURL: string = `/api/erhaltungsaufwendungen`;

    constructor(
        public id: number,
        public betrag: number,
        public datum: string,
        public notiz: string,
        public bezeichnung: string,
        public wohnung: WalterSelectionEntry,
        public aussteller: WalterSelectionEntry) {
        super();
    }

    static fromJson(json: any) {
        const wohnung = json.wohnung
            && WalterSelectionEntry.fromJson(json.wohnung);
        const aussteller = json.aussteller
            && WalterSelectionEntry.fromJson(json.aussteller);

        return new WalterErhaltungsaufwendungEntry(
            json.id,
            json.betrag,
            json.datum,
            json.notiz,
            json.bezeichnung,
            wohnung,
            aussteller);
    }
};