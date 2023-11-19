import { WalterApiHandler } from './WalterApiHandler';
import { WalterBetriebskostenrechnungEntry } from './WalterBetriebskostenrechnung';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterWohnungEntry } from './WalterWohnung';
import { WalterZaehlerEntry } from './WalterZaehler';

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
        public lastModified: Date
    ) {
        super();
    }

    static fromJson(json: any) {
        const hkvo_p9 =
            json.hkvO_P9 && WalterSelectionEntry.fromJson(json.hkvO_P9);
        const stromrechnung =
            json.stromrechnung &&
            WalterSelectionEntry.fromJson(json.stromrechnung);

        return new WalterHKVOEntry(
            json.id,
            json.notiz,
            json.hkvO_P7,
            json.hkvO_P8,
            hkvo_p9,
            json.strompauschale,
            stromrechnung,
            json.createdAt,
            json.lastModified
        );
    }
}
