import { WalterApiHandler } from './WalterApiHandler';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterMietminderungEntry extends WalterApiHandler {
    public static ApiURL = `/api/mietminderungen`;

    constructor(
        public id: number,
        public beginn: string,
        public ende: string | undefined,
        public minderung: number,
        public notiz: string,
        public vertrag: WalterSelectionEntry
    ) {
        super();
    }

    static fromJson(json: any) {
        const vertrag =
            json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);

        return new WalterMietminderungEntry(
            json.id,
            json.beginn,
            json.ende,
            json.minderung,
            json.notiz,
            vertrag
        );
    }
}
