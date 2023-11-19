import { WalterApiHandler } from './WalterApiHandler';
import { WalterPersonEntry } from './WalterPerson';
import { WalterWohnungEntry } from './WalterWohnung';
import { WalterZaehlerEntry } from './WalterZaehler';

export class WalterAdresseEntry extends WalterApiHandler {
    public static ApiURL = `/api/adressen`;

    constructor(
        public id: number,
        public strasse: string,
        public hausnummer: string,
        public postleitzahl: string,
        public stadt: string,
        public anschrift: string,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public wohnungen: WalterWohnungEntry[],
        public kontakte: WalterPersonEntry[],
        public zaehler: WalterZaehlerEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterAdresseEntry): WalterAdresseEntry {
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const kontakte = json.kontakte?.map(WalterPersonEntry.fromJson);
        const zaehler = json.zaehler?.map(WalterZaehlerEntry.fromJson);

        return new WalterAdresseEntry(
            json.id,
            json.strasse,
            json.hausnummer,
            json.postleitzahl,
            json.stadt,
            json.anschrift,
            json.notiz,
            json.createdAt,
            json.lastModified,
            wohnungen,
            kontakte,
            zaehler
        );
    }
}
