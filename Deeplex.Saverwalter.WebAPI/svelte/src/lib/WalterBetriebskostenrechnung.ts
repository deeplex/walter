import { WalterApiHandler } from './WalterApiHandler';
import { WalterSelectionEntry } from './WalterSelection';
import { WalterWohnungEntry } from './WalterWohnung';

export class WalterBetriebskostenrechnungEntry extends WalterApiHandler {
  public static ApiURL = `/api/betriebskostenrechnungen`;

  constructor(
    public id: number,
    public betrag: number,
    public betreffendesJahr: number,
    public datum: string,
    public notiz: string,
    public typ: WalterSelectionEntry,
    public umlage: WalterSelectionEntry,
    public wohnungen: WalterWohnungEntry[]
  ) {
    super();
  }

  public static fromJson(json: any): WalterBetriebskostenrechnungEntry {
    const typ = json.typ && WalterSelectionEntry.fromJson(json.typ);
    const umlage = json.umlage && WalterSelectionEntry.fromJson(json.umlage);
    const wohnungen = json.wohnungen?.map(WalterWohnungEntry.fromJson);

    return new WalterBetriebskostenrechnungEntry(
      json.id,
      json.betrag,
      json.betreffendesJahr,
      json.datum,
      json.notiz,
      typ,
      umlage,
      wohnungen
    );
  }
}
