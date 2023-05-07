import { WalterAdresseEntry } from './WalterAdresse';
import { WalterApiHandler } from './WalterApiHandler';
import type { WalterPersonEntry } from './WalterPerson';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterVertragEntry } from './WalterVertrag';
import { WalterWohnungEntry } from './WalterWohnung';

export class WalterJuristischePersonEntry
  extends WalterApiHandler
  implements WalterPersonEntry {
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
    public selectedJuristischePersonen: WalterSelectionEntry[],
    public natuerlichePerson: boolean,
    public adresse: WalterAdresseEntry,
    public juristischePersonen: WalterPersonEntry[],
    public wohnungen: WalterWohnungEntry[],
    public vertraege: WalterVertragEntry[],
    public selectedMitglieder: WalterSelectionEntry[],
    public mitglieder: WalterPersonEntry[]
  ) {
    super();
  }

  static fromJson(json: any) {
    const selectedMitglieder = json.selectedMitglieder?.map(
      WalterSelectionEntry.fromJson
    );
    const mitglieder = json.mitglieder?.map(WalterSelectionEntry.fromJson);
    const selectedJuristischePersonen = json.selectedJuristischePersonen?.map(
      WalterSelectionEntry.fromJson
    );
    const adresse = json.adresse && WalterAdresseEntry.fromJson(json.adresse);
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
      selectedJuristischePersonen,
      json.natuerlichePerson,
      adresse,
      juristischePersonen,
      wohnungen,
      vertraege,
      selectedMitglieder,
      mitglieder
    );
  }
}
