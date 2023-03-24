import { WalterAdresseEntry, WalterJuristischePersonEntry, WalterPersonEntry, WalterSelectionEntry, WalterVertragEntry, WalterWohnungEntry } from "$WalterLib";
import { WalterApiHandler } from "./WalterApiHandler";

export class WalterNatuerlichePersonEntry extends WalterApiHandler implements WalterPersonEntry {
    public static ApiURL: string = `/api/kontakte`;

    constructor(
        public id: number,
        public guid: string,
        public email: string,
        public telefon: string,
        public fax: string,
        public mobil: string,
        public notiz: string,
        public name: string,
        public selectedJuristischePersonen: WalterSelectionEntry[],
        public natuerlichePerson: boolean,
        public adresse: WalterAdresseEntry,
        public juristischePersonen: WalterPersonEntry[],
        public wohnungen: WalterWohnungEntry[],
        public vertraege: WalterVertragEntry[],
        public vorname: string,
        public nachname: string) {
        super();
    }

    static fromJson(json: any) {
        const selectedJuristischePersonen = json.selectedJuristischePersonen
            ?.map(WalterSelectionEntry.fromJson);
        const adresse = json.adresse
            && WalterAdresseEntry.fromJson(json.adresse);
        const juristischePersonen = json.juristischePersonen
            ?.map(WalterJuristischePersonEntry.fromJson);
        const wohnungen = json.wohnungen
            ?.map(WalterWohnungEntry.fromJson);
        const vertraege = json.vertraege
            ?.map(WalterVertragEntry.fromJson);

        return new WalterNatuerlichePersonEntry(
            json.id,
            json.guid,
            json.email,
            json.telefon,
            json.fax,
            json.mobil,
            json.notiz,
            json.name,
            selectedJuristischePersonen,
            json.natuerlichePerson,
            adresse,
            juristischePersonen,
            wohnungen,
            vertraege,
            json.vorname,
            json.nachname);
    }

}