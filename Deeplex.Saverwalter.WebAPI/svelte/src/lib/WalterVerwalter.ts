import { WalterApiHandler } from './WalterApiHandler';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterVerwalterEntry extends WalterApiHandler {
    // public static ApiURL = `/api/verwalter;

    constructor(
        public id: number,
        public rolle: WalterSelectionEntry,
        public wohnung: WalterSelectionEntry,
        public createdAt: Date,
        public lastModified: Date
    ) {
        super();
    }

    static fromJson(json: WalterVerwalterEntry) {
        const rolle = json.rolle && WalterSelectionEntry.fromJson(json.rolle);
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);

        return new WalterVerwalterEntry(
            json.id,
            rolle,
            wohnung,
            json.createdAt,
            json.lastModified
        );
    }
}
