import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterVertragVersionEntry extends WalterApiHandler {
    public static ApiURL = `/api/vertragversionen`;

    constructor(
        public id: number,
        public beginn: string,
        public personenzahl: number,
        public notiz: string,
        public grundmiete: number,
        public createdAt: Date,
        public lastModified: Date,
        public vertrag: WalterSelectionEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterVertragVersionEntry) {
        const vertrag =
            json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterVertragVersionEntry(
            json.id,
            json.beginn,
            json.personenzahl,
            json.notiz,
            json.grundmiete,
            json.createdAt,
            json.lastModified,
            vertrag,
            permissions
        );
    }
}
