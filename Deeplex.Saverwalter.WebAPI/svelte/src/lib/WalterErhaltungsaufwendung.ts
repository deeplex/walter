import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterErhaltungsaufwendungEntry extends WalterApiHandler {
    public static ApiURL = `/api/erhaltungsaufwendungen`;
    public static ApiURLId(id: number) {
        return `${WalterErhaltungsaufwendungEntry.ApiURL}/${id}`;
    }

    constructor(
        public id: number,
        public betrag: number,
        public datum: string,
        public notiz: string,
        public bezeichnung: string,
        public createdAt: Date,
        public lastModified: Date,
        public wohnung: WalterSelectionEntry,
        public aussteller: WalterSelectionEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterErhaltungsaufwendungEntry) {
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const aussteller =
            json.aussteller && WalterSelectionEntry.fromJson(json.aussteller);
        const permissions = WalterPermissions.fromJson(json.permissions);

        return new WalterErhaltungsaufwendungEntry(
            json.id,
            json.betrag,
            json.datum,
            json.notiz,
            json.bezeichnung,
            json.createdAt,
            json.lastModified,
            wohnung,
            aussteller,
            permissions
        );
    }
}
