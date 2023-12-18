import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import { WalterPermissions } from './WalterPermissions';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVertragEntry } from './WalterVertrag';
import { WalterWohnungEntry } from './WalterWohnung';

export class WalterKontaktEntry extends WalterApiHandler {
    public static ApiURL = `/api/kontakte`;

    constructor(
        public id: number,
        public email: string,
        public telefon: string,
        public fax: string,
        public mobil: string,
        public notiz: string,
        public rechtsform: WalterSelectionEntry,
        public bezeichnung: string,
        public name: string,
        public vorname: string,
        public createdAt: Date,
        public lastModified: Date,
        public anrede: WalterSelectionEntry,
        public selectedJuristischePersonen: WalterSelectionEntry[],
        public adresse: WalterAdresseEntry | undefined,
        public juristischePersonen: WalterKontaktEntry[],
        public wohnungen: WalterWohnungEntry[],
        public vertraege: WalterVertragEntry[],
        public selectedMitglieder: WalterSelectionEntry[],
        public mitglieder: WalterKontaktEntry[],
        public permissions: WalterPermissions
    ) {
        super();
    }

    static fromJson(json: WalterKontaktEntry): WalterKontaktEntry {
        const selectedJuristischePersonen =
            json.selectedJuristischePersonen?.map(
                WalterSelectionEntry.fromJson
            );
        const adresse =
            json.adresse && WalterAdresseEntry.fromJson(json.adresse);
        const juristischePersonen = json.juristischePersonen?.map(
            WalterKontaktEntry.fromJson
        );
        const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
        const vertraege = json.vertraege?.map(WalterVertragEntry.fromJson);
        const mitglieder = json.mitglieder?.map(WalterKontaktEntry.fromJson);
        const permissions =
            json.permissions && WalterPermissions.fromJson(json.permissions);
        const rechtsform =
            json.rechtsform && WalterSelectionEntry.fromJson(json.rechtsform);
        const selectedMitglieder = json.selectedMitglieder?.map(
            WalterSelectionEntry.fromJson
        );

        return new WalterKontaktEntry(
            json.id,
            json.email,
            json.telefon,
            json.fax,
            json.mobil,
            json.notiz,
            rechtsform,
            json.bezeichnung,
            json.name,
            json.vorname,
            json.createdAt,
            json.lastModified,
            json.anrede,
            selectedJuristischePersonen,
            adresse,
            juristischePersonen,
            wohnungen,
            vertraege,
            selectedMitglieder,
            mitglieder,
            permissions
        );
    }
}
