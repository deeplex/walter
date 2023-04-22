import {
  WalterAdresseEntry,
  WalterJuristischePersonEntry,
  WalterSelectionEntry,
  WalterVertragEntry,
  WalterWohnungEntry
} from '$WalterLib';
import { WalterApiHandler } from './WalterApiHandler';

export class WalterPersonEntry extends WalterApiHandler {
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
    public adresse: WalterAdresseEntry | undefined,
    public juristischePersonen: WalterPersonEntry[],
    public wohnungen: WalterWohnungEntry[],
    public vertraege: WalterVertragEntry[]
  ) {
    super();
  }

  static fromJson(json: any) {
    const adresse = json.adresse && WalterAdresseEntry.fromJson(json.adresse);
    const selectedJuristischePersonen = json.selectedJuristischePersonen?.map(
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
      selectedJuristischePersonen,
      json.natuerlichePerson,
      adresse,
      juristischePersonen,
      wohnungen,
      vertraege
    );
  }
}
