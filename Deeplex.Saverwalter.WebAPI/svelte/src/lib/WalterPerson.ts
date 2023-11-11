import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterJuristischePersonEntry } from './WalterJuristischePerson';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVertragEntry } from './WalterVertrag';
import { WalterWohnungEntry } from './WalterWohnung';

export class WalterPersonEntry extends WalterApiHandler {
    public static ApiURL = `/api/kontakte`;

    constructor(
        public id: number,
        public guid: string,
        public email: string,
        public telefon: string,
        public fax: string,
        public mobil: string,
        public notiz: string,
        public name: string,
        public createdAt: Date,
        public lastModified: Date,
        public selectedJuristischePersonen: WalterSelectionEntry[],
        public natuerlichePerson: boolean,
        public adresse: WalterAdresseEntry | undefined,
        public juristischePersonen: WalterPersonEntry[],
        public wohnungen: WalterWohnungEntry[],
        public vertraege: WalterVertragEntry[]
    ) {
        super();
    }

    static fromJson(json: any) {
        const adresse =
            json.adresse && WalterAdresseEntry.fromJson(json.adresse);
        const selectedJuristischePersonen =
            json.selectedJuristischePersonen?.map(
                WalterSelectionEntry.fromJson
            );
        const juristischePersonen = json.juristischePersonen?.map(
            WalterJuristischePersonEntry.fromJson
        );
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const vertraege = json.vertraege?.map(WalterVertragEntry.fromJson);

        return new WalterPersonEntry(
            json.id,
            json.guid,
            json.email,
            json.telefon,
            json.fax,
            json.mobil,
            json.notiz,
            json.name,
            json.createdAt,
            json.lastModified,
            selectedJuristischePersonen,
            json.natuerlichePerson,
            adresse,
            juristischePersonen,
            wohnungen,
            vertraege
        );
    }
}
