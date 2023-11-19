import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterJuristischePersonEntry } from './WalterJuristischePerson';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVertragEntry } from './WalterVertrag';
import { WalterWohnungEntry } from './WalterWohnung';
import type { WalterPersonEntry } from './WalterPerson';

export class WalterNatuerlichePersonEntry
    extends WalterApiHandler
    implements WalterPersonEntry
{
    public static ApiURL = `/api/kontakte/nat`;

    constructor(
        public id: number,
        public guid: string,
        public email: string,
        public telefon: string,
        public fax: string,
        public mobil: string,
        public notiz: string,
        public name: string,
        public vorname: string,
        public nachname: string,
        public createdAt: Date,
        public lastModified: Date,
        public anrede: WalterSelectionEntry,
        public selectedJuristischePersonen: WalterSelectionEntry[],
        public natuerlichePerson: boolean,
        public adresse: WalterAdresseEntry,
        public juristischePersonen: WalterJuristischePersonEntry[],
        public wohnungen: WalterWohnungEntry[],
        public vertraege: WalterVertragEntry[]
    ) {
        super();
    }

    static fromJson(json: WalterNatuerlichePersonEntry) {
        const selectedJuristischePersonen =
            json.selectedJuristischePersonen?.map(
                WalterSelectionEntry.fromJson
            );
        const adresse =
            json.adresse && WalterAdresseEntry.fromJson(json.adresse);
        const juristischePersonen = json.juristischePersonen?.map(
            WalterJuristischePersonEntry.fromJson
        );
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const vertraege = json.vertraege?.map(WalterVertragEntry.fromJson);

        return new WalterNatuerlichePersonEntry(
            json.id,
            json.guid,
            json.email,
            json.telefon,
            json.fax,
            json.mobil,
            json.notiz,
            json.name,
            json.vorname,
            json.nachname,
            json.createdAt,
            json.lastModified,
            json.anrede,
            selectedJuristischePersonen,
            json.natuerlichePerson,
            adresse,
            juristischePersonen,
            wohnungen,
            vertraege
        );
    }
}
