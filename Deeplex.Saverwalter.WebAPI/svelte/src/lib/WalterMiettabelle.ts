import { WalterApiHandler } from './WalterApiHandler';
import { WalterMieteEntry } from './WalterMiete';

export class WalterMiettabelleEntry extends WalterApiHandler {
    public static ApiURL = `/api/miettabelle`;

    constructor(
        public id: number,
        public bezeichnung: string,
        public createdAt: Date,
        public lastModified: Date,
        public mieten: WalterMieteEntry[]
    ) {
        super();
    }

    static fromJson(json: any) {
        const mieten =
            json.mieten && json.mieten.map(WalterMieteEntry.fromJson);

        return new WalterMiettabelleEntry(
            json.id,
            json.bezeichnung,
            json.createdAt,
            json.lastModified,
            mieten
        );
    }
}
