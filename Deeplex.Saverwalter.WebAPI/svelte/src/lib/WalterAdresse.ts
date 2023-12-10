import { WalterApiHandler } from './WalterApiHandler';
import { WalterKontaktEntry } from './WalterKontakt';
import { WalterPermissions } from './WalterPermissions';
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
        public kontakte: WalterKontaktEntry[],
        public zaehler: WalterZaehlerEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterAdresseEntry): WalterAdresseEntry {
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const kontakte = json.kontakte?.map(WalterKontaktEntry.fromJson);
        const zaehler = json.zaehler?.map(WalterZaehlerEntry.fromJson);
        const permissions = WalterPermissions.fromJson(json.permissions);

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
            zaehler,
            permissions
        );
    }
}
