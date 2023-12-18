import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterHKVOEntry extends WalterApiHandler {
    // public static ApiURL = `/api/hkvo`;

    constructor(
        public id: number,
        public notiz: string,
        public hkvO_P7: number,
        public hkvO_P8: number,
        public hkvO_P9: WalterSelectionEntry,
        public strompauschale: number,
        public stromrechnung: WalterSelectionEntry,
        public createdAt: Date,
        public lastModified: Date,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterHKVOEntry) {
        const hkvo_p9 =
            json.hkvO_P9 && WalterSelectionEntry.fromJson(json.hkvO_P9);
        const stromrechnung =
            json.stromrechnung &&
            WalterSelectionEntry.fromJson(json.stromrechnung);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);

        return new WalterHKVOEntry(
            json.id,
            json.notiz,
            json.hkvO_P7,
            json.hkvO_P8,
            hkvo_p9,
            json.strompauschale,
            stromrechnung,
            json.createdAt,
            json.lastModified,
            permissions
        );
    }
}
