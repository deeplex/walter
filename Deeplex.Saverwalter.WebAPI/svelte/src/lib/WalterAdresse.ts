import {
  WalterPersonEntry,
  WalterWohnungEntry,
  WalterZaehlerEntry
} from '$WalterLib';
import { WalterApiHandler } from './WalterApiHandler';

export class WalterAdresseEntry extends WalterApiHandler {
  public static ApiURL: string = `/api/adressen`;

  constructor(
    public id: number,
    public strasse: string,
    public hausnummer: string,
    public postleitzahl: string,
    public stadt: string,
    public anschrift: string,
    public notiz: string,
    public wohnungen: WalterWohnungEntry[],
    public kontakte: WalterPersonEntry[],
    public zaehler: WalterZaehlerEntry[]
  ) {
    super();
  }

  static fromJson(json: any) {
    const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);
    const kontakte = json.kontakte?.map(WalterPersonEntry.fromJson);
    const zaehler = json.zaehler?.map(WalterZaehlerEntry.fromJson);

    return new WalterAdresseEntry(
      json.id,
      json.strasse,
      json.hausnummer,
      json.postleitzahl,
      json.stadt,
      json.anschrift,
      json.notiz,
      wohnungen,
      kontakte,
      zaehler
    );
  }
}
