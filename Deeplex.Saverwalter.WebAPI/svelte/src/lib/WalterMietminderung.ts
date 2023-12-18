import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterMietminderungEntry extends WalterApiHandler {
    public static ApiURL = `/api/mietminderungen`;

    constructor(
        public id: number,
        public beginn: string,
        public ende: string | undefined,
        public minderung: number,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public vertrag: WalterSelectionEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterMietminderungEntry) {
        const vertrag =
            json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterMietminderungEntry(
            json.id,
            json.beginn,
            json.ende,
            json.minderung,
            json.notiz,
            json.createdAt,
            json.lastModified,
            vertrag,
            permissions
        );
    }
}
