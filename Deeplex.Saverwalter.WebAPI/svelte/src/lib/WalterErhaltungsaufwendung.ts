import { WalterApiHandler } from './WalterApiHandler';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterErhaltungsaufwendungEntry extends WalterApiHandler {
    public static ApiURL = `/api/erhaltungsaufwendungen`;

    constructor(
        public id: number,
        public betrag: number,
        public datum: string,
        public notiz: string,
        public bezeichnung: string,
        public createdAt: Date,
        public lastModified: Date,
        public wohnung: WalterSelectionEntry,
        public aussteller: WalterSelectionEntry
    ) {
        super();
    }

    static fromJson(json: WalterErhaltungsaufwendungEntry) {
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const aussteller =
            json.aussteller && WalterSelectionEntry.fromJson(json.aussteller);

        return new WalterErhaltungsaufwendungEntry(
            json.id,
            json.betrag,
            json.datum,
            json.notiz,
            json.bezeichnung,
            json.createdAt,
            json.lastModified,
            wohnung,
            aussteller
        );
    }
}
