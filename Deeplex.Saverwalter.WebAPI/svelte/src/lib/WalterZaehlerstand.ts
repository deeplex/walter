import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterZaehlerstandEntry extends WalterApiHandler {
    public static ApiURL = `/api/zaehlerstaende`;

    constructor(
        public id: number,
        public stand: number,
        public datum: string,
        public einheit: string,
        public zaehler: WalterSelectionEntry,
        public notiz: string,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterZaehlerstandEntry) {
        const zaehler =
            json.zaehler && WalterSelectionEntry.fromJson(json.zaehler);
        const permissions = WalterPermissions.fromJson(json.permissions);

        return new WalterZaehlerstandEntry(
            json.id,
            json.stand,
            json.datum,
            json.einheit,
            zaehler,
            json.notiz,
            permissions
        );
    }
}
