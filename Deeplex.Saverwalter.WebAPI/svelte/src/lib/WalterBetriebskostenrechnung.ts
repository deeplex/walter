import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterWohnungEntry } from './WalterWohnung';

export class WalterBetriebskostenrechnungEntry extends WalterApiHandler {
    public static ApiURL = `/api/betriebskostenrechnungen`;

    constructor(
        public id: number,
        public betrag: number,
        public betreffendesJahr: number,
        public datum: string,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public typ: WalterSelectionEntry,
        public umlage: WalterSelectionEntry,
        public wohnungen: WalterWohnungEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    public static fromJson(
        json: WalterBetriebskostenrechnungEntry
    ): WalterBetriebskostenrechnungEntry {
        const typ = json.typ && WalterSelectionEntry.fromJson(json.typ);
        const umlage =
            json.umlage && WalterSelectionEntry.fromJson(json.umlage);
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
            WalterBetriebskostenrechnungEntry.fromJson
        );
        const permissions = WalterPermissions.fromJson(json.permissions);

        return new WalterBetriebskostenrechnungEntry(
            json.id,
            json.betrag,
            json.betreffendesJahr,
            json.datum,
            json.notiz,
            json.createdAt,
            json.lastModified,
            typ,
            umlage,
            wohnungen,
            betriebskostenrechnungen,
            permissions
        );
    }
}
