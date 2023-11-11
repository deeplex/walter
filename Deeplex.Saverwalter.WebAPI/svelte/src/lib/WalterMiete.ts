import { WalterApiHandler } from './WalterApiHandler';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterMieteEntry extends WalterApiHandler {
    public static ApiURL = `/api/mieten`;

    constructor(
        public id: number,
        public betreffenderMonat: string,
        public zahlungsdatum: string,
        public betrag: number,
        public notiz: string,
        public repeat: number,
        public createdAt: Date,
        public lastModified: Date,
        public vertrag: WalterSelectionEntry
    ) {
        super();
    }

    static fromJson(json: any) {
        const vertrag =
            json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);

        return new WalterMieteEntry(
            json.id,
            json.betreffenderMonat,
            json.zahlungsdatum,
            json.betrag,
            json.notiz,
            json.repeat,
            json.createdAt,
            json.lastModified,
            vertrag
        );
    }
}
