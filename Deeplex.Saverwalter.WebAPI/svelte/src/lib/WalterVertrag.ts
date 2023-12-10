import { WalterApiHandler } from './WalterApiHandler';
import { WalterBetriebskostenrechnungEntry } from './WalterBetriebskostenrechnung';
import { WalterKontaktEntry } from './WalterKontakt';
import { WalterMieteEntry } from './WalterMiete';
import { WalterMietminderungEntry } from './WalterMietminderung';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVertragVersionEntry } from './WalterVertragVersion';

export class WalterVertragEntry extends WalterApiHandler {
    public static ApiURL = `/api/vertraege`;

    constructor(
        public id: number,
        public beginn: string,
        public ende: string | undefined,
        public notiz: string,
        public mieterAuflistung: string,
        public createdAt: Date,
        public lastModified: Date,
        public wohnung: WalterSelectionEntry,
        public ansprechpartner: WalterSelectionEntry,
        public selectedMieter: WalterSelectionEntry[],
        public versionen: WalterVertragVersionEntry[],
        public mieter: WalterKontaktEntry[],
        public mieten: WalterMieteEntry[],
        public mietminderungen: WalterMietminderungEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterVertragEntry) {
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const ansprechpartner =
            json.ansprechpartner &&
            WalterSelectionEntry.fromJson(json.ansprechpartner);
        const selectedMieter = json.selectedMieter?.map(
            WalterSelectionEntry.fromJson
        );
        const versionen = json.versionen?.map(
            WalterVertragVersionEntry.fromJson
        );
        const mieter = json.mieter?.map(WalterKontaktEntry.fromJson);
        const mieten = json.mieten?.map(WalterMieteEntry.fromJson);
        const mietminderungen = json.mietminderungen?.map(
            WalterMietminderungEntry.fromJson
        );
        const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
            WalterBetriebskostenrechnungEntry.fromJson
        );
        const permissions = WalterPermissions.fromJson(json.permissions);

        return new WalterVertragEntry(
            json.id,
            json.beginn,
            json.ende,
            json.notiz,
            json.mieterAuflistung,
            json.createdAt,
            json.lastModified,
            wohnung,
            ansprechpartner,
            selectedMieter,
            versionen,
            mieter,
            mieten,
            mietminderungen,
            betriebskostenrechnungen,
            permissions
        );
    }
}
