import { WalterApiHandler } from './WalterApiHandler';
import { WalterMieteEntry } from './WalterMiete';

export class WalterPermissions {
    constructor(
        public read: boolean,
        public update: boolean,
        public remove: boolean
    ) {}

    static fromJson(json: WalterPermissions) {
        return new WalterPermissions(json.read, json.update, json.remove);
    }
}
