import { WalterApiHandler } from './WalterApiHandler';
import { WalterKontaktEntry } from './WalterKontakt';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterWohnungEntry } from './WalterWohnung';

export class WalterAccountEntry extends WalterApiHandler {
    public static ApiURL = `/api/accounts`;
    public static ApiURLUser = `/api/user`;

    constructor(
        public id: number,
        public username: string,
        public name: string,
        public createdAt: Date,
        public lastModified: Date,
        public selectedKontakte: WalterSelectionEntry[],
        public selectedWohnungen: WalterSelectionEntry[],
        public kontakte: WalterKontaktEntry[],
        public wohnungen: WalterWohnungEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterAccountEntry): WalterAccountEntry {
        const selectedKontakte = json.selectedKontakte?.map(
            WalterSelectionEntry.fromJson
        );
        const selectedWohnungen = json.selectedWohnungen?.map(
            WalterSelectionEntry.fromJson
        );
        const kontakte = json.kontakte?.map(WalterKontaktEntry.fromJson);
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);

        return new WalterAccountEntry(
            json.id,
            json.username,
            json.name,
            json.createdAt,
            json.lastModified,
            selectedKontakte,
            selectedWohnungen,
            kontakte,
            wohnungen
        );
    }
}
