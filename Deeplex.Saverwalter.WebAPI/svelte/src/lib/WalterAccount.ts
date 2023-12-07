import { WalterApiHandler } from './WalterApiHandler';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterAccountEntry extends WalterApiHandler {
    public static ApiURL = `/api/accounts`;
    public static ApiURLUser = `/api/user`;

    constructor(
        public id: number,
        public username: string,
        public name: string,
        public createdAt: Date,
        public lastModified: Date,
        public kontakte: WalterSelectionEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterAccountEntry): WalterAccountEntry {
        const kontakte = json.kontakte?.map(WalterSelectionEntry.fromJson);

        return new WalterAccountEntry(
            json.id,
            json.username,
            json.name,
            json.createdAt,
            json.lastModified,
            kontakte
        );
    }
}
