import {
  WalterAdresseEntry,
  WalterSelectionEntry,
  WalterZaehlerstandEntry
} from '$WalterLib';
import { WalterApiHandler } from './WalterApiHandler';

export class WalterZaehlerEntry extends WalterApiHandler {
  public static ApiURL: string = `/api/zaehler`;

  constructor(
    public id: number,
    public kennnummer: string,
    public adresse: WalterAdresseEntry,
    public typ: WalterSelectionEntry | undefined,
    public allgemeinZaehler: WalterSelectionEntry | undefined,
    public wohnung: WalterSelectionEntry | undefined,
    public notiz: string,
    public staende: WalterZaehlerstandEntry[],
    public einzelzaehler: WalterZaehlerEntry[]
  ) {
    super();
  }

  static fromJson(json: any) {
    const adresse = json.adresse && WalterAdresseEntry.fromJson(json.adresse);
    const typ = json.typ && WalterSelectionEntry.fromJson(json.typ);
    const allgemeinZaehler =
      json.allgemeinZaehler &&
      WalterSelectionEntry.fromJson(json.allgemeinZaehler);
    const wohnung = json.wohnung && WalterSelectionEntry.fromJson(json.wohnung);
    const staende = json.staende?.map(WalterZaehlerstandEntry.fromJson);
    const einzelzaehler = json.einzelzaehler?.map(WalterZaehlerEntry.fromJson);

    return new WalterZaehlerEntry(
      json.id,
      json.kennnummer,
      adresse,
      typ,
      allgemeinZaehler,
      wohnung,
      json.notiz,
      staende,
      einzelzaehler
    );
  }
}
