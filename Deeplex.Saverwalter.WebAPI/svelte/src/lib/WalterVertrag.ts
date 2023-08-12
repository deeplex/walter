import { WalterApiHandler } from './WalterApiHandler';
import { WalterBetriebskostenrechnungEntry } from './WalterBetriebskostenrechnung';
import { WalterMieteEntry } from './WalterMiete';
import { WalterMietminderungEntry } from './WalterMietminderung';
import { WalterPersonEntry } from './WalterPerson';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVertragVersionEntry } from './WalterVertragVersion';

export class WalterVertragEntry extends WalterApiHandler {
    public static ApiURL = `/api/vertraege`;

    constructor(
        public id: number,
        public beginn: string,
        public ende: string | undefined,
        public wohnung: WalterSelectionEntry,
        public ansprechpartner: WalterSelectionEntry | undefined,
        public selectedMieter: WalterSelectionEntry[],
        public notiz: string,
        public mieterAuflistung: string,
        public versionen: WalterVertragVersionEntry[],
        public mieter: WalterPersonEntry[],
        public mieten: WalterMieteEntry[],
        public mietminderungen: WalterMietminderungEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[],
        public lastMiete: WalterMieteEntry
    ) {
        super();
    }

    static fromJson(json: any) {
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
        const mieter = json.mieter?.map(WalterPersonEntry.fromJson);
        const mieten = json.mieten?.map(WalterMieteEntry.fromJson);
        const mietminderungen = json.mietminderungen?.map(
            WalterMietminderungEntry.fromJson
        );
        const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
            WalterBetriebskostenrechnungEntry.fromJson
        );
        const lastMiete =
            json.lastMiete && WalterMieteEntry.fromJson(json.lastMiete);

        return new WalterVertragEntry(
            json.id,
            json.beginn,
            json.ende,
            wohnung,
            ansprechpartner,
            selectedMieter,
            json.notiz,
            json.mieterAuflistung,
            versionen,
            mieter,
            mieten,
            mietminderungen,
            betriebskostenrechnungen,
            lastMiete
        );
    }
}
