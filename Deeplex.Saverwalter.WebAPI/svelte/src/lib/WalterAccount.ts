import { WalterApiHandler } from './WalterApiHandler';
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
        public selectedWohnungen: WalterSelectionEntry[],
        public wohnungen: WalterWohnungEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterAccountEntry): WalterAccountEntry {
        const selectedWohnungen = json.selectedWohnungen?.map(
            WalterSelectionEntry.fromJson
        );
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);

        return new WalterAccountEntry(
            json.id,
            json.username,
            json.name,
            json.createdAt,
            json.lastModified,
            selectedWohnungen,
            wohnungen
        );
    }
}
