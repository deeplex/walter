import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterVerwalterEntry extends WalterApiHandler {
    // public static ApiURL = `/api/verwalter;

    constructor(
        public id: number,
        public rolle: WalterSelectionEntry,
        public wohnung: WalterSelectionEntry,
        public createdAt: Date,
        public lastModified: Date,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterVerwalterEntry) {
        const rolle = json.rolle && WalterSelectionEntry.fromJson(json.rolle);
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const permissions = WalterPermissions.fromJson(json.permissions);

        return new WalterVerwalterEntry(
            json.id,
            rolle,
            wohnung,
            json.createdAt,
            json.lastModified,
            permissions
        );
    }
}
