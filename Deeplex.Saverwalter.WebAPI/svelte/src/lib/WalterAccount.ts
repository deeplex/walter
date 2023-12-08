import { WalterApiHandler } from './WalterApiHandler';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVerwalterEntry } from './WalterVerwalter';

export class WalterAccountEntry extends WalterApiHandler {
    public static ApiURL = `/api/accounts`;
    public static ApiURLUser = `/api/user`;

    constructor(
        public id: number,
        public username: string,
        public name: string,
        public role: WalterSelectionEntry,
        public createdAt: Date,
        public lastModified: Date,
        public resetToken: string,
        public resetTokenExpires: Date,
        public verwalter: WalterVerwalterEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterAccountEntry): WalterAccountEntry {
        const verwalter = json.verwalter?.map(WalterVerwalterEntry.fromJson);
        const role = WalterSelectionEntry.fromJson(json.role);

        return new WalterAccountEntry(
            json.id,
            json.username,
            json.name,
            role,
            json.createdAt,
            json.lastModified,
            json.resetToken,
            json.resetTokenExpires,
            verwalter
        );
    }
}
