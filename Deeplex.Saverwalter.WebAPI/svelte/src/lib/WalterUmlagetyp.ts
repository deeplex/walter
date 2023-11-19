import { WalterApiHandler } from './WalterApiHandler';
import { WalterUmlageEntry } from './WalterUmlage';

export class WalterUmlagetypEntry extends WalterApiHandler {
    public static ApiURL = `/api/umlagetypen`;

    constructor(
        public id: number,
        public notiz: string,
        public bezeichnung: string,
        public createdAt: Date,
        public lastModified: Date,
        public umlagen: WalterUmlageEntry[]
    ) {
        super();
    }

    static fromJson(json: any) {
        const umlagen = json.umlagen?.map(WalterUmlageEntry.fromJson);

        return new WalterUmlagetypEntry(
            json.id,
            json.notiz,
            json.bezeichnung,
            json.createdAt,
            json.lastModified,
            umlagen
        );
    }
}
