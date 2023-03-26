import { WalterApiHandler } from "./WalterApiHandler";
import { WalterSelectionEntry } from "./WalterSelection";

export class WalterZaehlerstandEntry extends WalterApiHandler {
    public static ApiURL: string = `/api/zaehlerstaende`;

    constructor(
        public id: number,
        public stand: number,
        public datum: string,
        public einheit: string,
        public zaehler: WalterSelectionEntry,
        public notiz: string) {
        super();
    }

    static fromJson(json: any) {
        const zaehler = json.zaehler
            && WalterSelectionEntry.fromJson(json.zaehler);

        return new WalterZaehlerstandEntry(
            json.id,
            json.stand,
            json.datum,
            json.einheit,
            zaehler,
            json.notiz);
    }
}