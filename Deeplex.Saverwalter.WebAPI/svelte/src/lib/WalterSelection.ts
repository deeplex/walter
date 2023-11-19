import { WalterApiHandler } from './WalterApiHandler';

export class WalterSelectionEntry extends WalterApiHandler {
    constructor(
        public id: number | string,
        public text: string,
        public filter?: string
    ) {
        super();
    }

    static fromJson(json: WalterSelectionEntry) {
        return new WalterSelectionEntry(json.id, json.text, json.filter);
    }
}
