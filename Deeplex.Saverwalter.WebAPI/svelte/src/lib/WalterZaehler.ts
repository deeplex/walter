import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterUmlageEntry } from './WalterUmlage';
import { WalterZaehlerstandEntry } from './WalterZaehlerstand';

export class WalterZaehlerEntry extends WalterApiHandler {
    public static ApiURL = `/api/zaehler`;

    constructor(
        public id: number,
        public kennnummer: string,
        public ende: string,
        public notiz: string,
        public createdAt: Date,
        public lastModified: Date,
        public adresse: WalterAdresseEntry,
        public typ: WalterSelectionEntry | undefined,
        public wohnung: WalterSelectionEntry | undefined,
        public umlagen: WalterUmlageEntry[],
        public selectedUmlagen: WalterSelectionEntry[],
        public staende: WalterZaehlerstandEntry[],
        public lastZaehlerstand: WalterZaehlerstandEntry,
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterZaehlerEntry) {
        const adresse =
            json.adresse && WalterAdresseEntry.fromJson(json.adresse);
        const typ = json.typ && WalterSelectionEntry.fromJson(json.typ);
        const wohnung =
            json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
        const umlagen = json.umlagen?.map(WalterUmlageEntry.fromJson);
        const selectedUmlagen = json.selectedUmlagen?.map(
            WalterSelectionEntry.fromJson
        );
        const staende = json.staende?.map(WalterZaehlerstandEntry.fromJson);
        const lastZaehlerstand =
            json.lastZaehlerstand &&
            WalterZaehlerstandEntry.fromJson(json.lastZaehlerstand);
        const permissions = WalterPermissions.fromJson(json.permissions);

        return new WalterZaehlerEntry(
            json.id,
            json.kennnummer,
            json.ende,
            json.notiz,
            json.createdAt,
            json.lastModified,
            adresse,
            typ,
            wohnung,
            umlagen,
            selectedUmlagen,
            staende,
            lastZaehlerstand,
            permissions
        );
    }
}
