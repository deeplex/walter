import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterUmlageEntry } from './WalterUmlage';

export class WalterUmlagetypEntry extends WalterApiHandler {
    public static ApiURL = `/api/umlagetypen`;

    constructor(
        public id: number,
        public notiz: string,
        public bezeichnung: string,
        public createdAt: Date,
        public lastModified: Date,
        public umlagen: WalterUmlageEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterUmlagetypEntry) {
        const umlagen = json.umlagen?.map(WalterUmlageEntry.fromJson);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterUmlagetypEntry(
            json.id,
            json.notiz,
            json.bezeichnung,
            json.createdAt,
            json.lastModified,
            umlagen,
            permissions
        );
    }
}
