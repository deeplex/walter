import { WalterApiHandler } from './WalterApiHandler';
import { WalterBetriebskostenrechnungEntry } from './WalterBetriebskostenrechnung';
import { WalterHKVOEntry } from './WalterHKVO';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterWohnungEntry } from './WalterWohnung';
import { WalterZaehlerEntry } from './WalterZaehler';

export class WalterUmlageEntry extends WalterApiHandler {
    public static ApiURL = `/api/umlagen`;

    constructor(
        public id: number,
        public notiz: string,
        public beschreibung: string,
        public wohnungenBezeichnung: string,
        public hKVO: Partial<WalterHKVOEntry>,
        public createdAt: Date,
        public lastModified: Date,
        public zaehler: WalterZaehlerEntry[],
        public selectedZaehler: WalterSelectionEntry[],
        public typ: WalterSelectionEntry,
        public schluessel: WalterSelectionEntry,
        public selectedWohnungen: WalterSelectionEntry[],
        public wohnungen: WalterWohnungEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterUmlageEntry): WalterUmlageEntry {
        const typ = json.typ && WalterSelectionEntry.fromJson(json.typ);
        const schluessel =
            json.schluessel && WalterSelectionEntry.fromJson(json.schluessel);
        const selectedWohnungen = json.selectedWohnungen?.map(
            WalterSelectionEntry.fromJson
        );
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
            WalterBetriebskostenrechnungEntry.fromJson
        );
        const selectedZaehler = json.selectedZaehler?.map(
            WalterSelectionEntry.fromJson
        );
        const hkvo = json.hKVO && WalterHKVOEntry.fromJson(json.hKVO);
        const zaehler = json.zaehler?.map(WalterZaehlerEntry.fromJson);

        return new WalterUmlageEntry(
            json.id,
            json.notiz,
            json.beschreibung,
            json.wohnungenBezeichnung,
            hkvo,
            json.createdAt,
            json.lastModified,
            zaehler,
            selectedZaehler,
            typ,
            schluessel,
            selectedWohnungen,
            wohnungen,
            betriebskostenrechnungen
        );
    }
}
