import { WalterBetriebskostenrechnungEntry, WalterSelectionEntry, WalterWohnungEntry } from "$WalterLib";
import { WalterApiHandler } from "./WalterApiHandler";

export class WalterUmlageEntry extends WalterApiHandler {
    public static ApiURL: string = `/api/umlagen`;

    constructor(
        public id: number,
        public notiz: string,
        public beschreibung: string,
        public wohnungenBezeichnung: string,
        public zaehler: string,
        public typ: WalterSelectionEntry,
        public schluessel: WalterSelectionEntry,
        public selectedWohnungen: WalterSelectionEntry[],
        public wohnungen: WalterWohnungEntry[],
        public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[]) {
        super();
    }

    static fromJson(json: any) {
        const typ = json.typ
            && WalterSelectionEntry.fromJson(json.typ);
        const schluessel = json.schluessel
            && WalterSelectionEntry.fromJson(json.schluessel);
        const selectedWohnungen = json.selectedWohnungen
            ?.map(WalterSelectionEntry.fromJson);
        const wohnungen = json.wohnungen
            ?.map(WalterWohnungEntry.fromJson);
        const betriebskostenrechnungen = json.betriebskostenrechnungen
            ?.map(WalterBetriebskostenrechnungEntry.fromJson);

        return new WalterUmlageEntry(
            json.id,
            json.notiz,
            json.beschreibung,
            json.wohnungenBezeichnung,
            json.zaehler,
            typ,
            schluessel,
            selectedWohnungen,
            wohnungen,
            betriebskostenrechnungen);
    }
};