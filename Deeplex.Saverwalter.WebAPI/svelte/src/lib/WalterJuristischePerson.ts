import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterPersonEntry } from './WalterPerson';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVertragEntry } from './WalterVertrag';
import { WalterWohnungEntry } from './WalterWohnung';

export class WalterJuristischePersonEntry
    extends WalterApiHandler
    implements WalterPersonEntry
{
    public static ApiURL = `/api/kontakte/jur`;

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
        public natuerlichePerson: boolean,
        public selectedJuristischePersonen: WalterSelectionEntry[],
        public adresse: WalterAdresseEntry,
        public juristischePersonen: WalterJuristischePersonEntry[],
        public wohnungen: WalterWohnungEntry[],
        public vertraege: WalterVertragEntry[],
        public selectedMitglieder: WalterSelectionEntry[],
        public mitglieder: WalterPersonEntry[]
    ) {
        super();
    }

    static fromJson(
        json: WalterJuristischePersonEntry
    ): WalterJuristischePersonEntry {
        const selectedMitglieder = json.selectedMitglieder?.map(
            WalterSelectionEntry.fromJson
        );
        const mitglieder = json.mitglieder?.map(WalterPersonEntry.fromJson);
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

        return new WalterJuristischePersonEntry(
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
            json.natuerlichePerson,
            selectedJuristischePersonen,
            adresse,
            juristischePersonen,
            wohnungen,
            vertraege,
            selectedMitglieder,
            mitglieder
        );
    }
}
