import {
  WalterAdresseEntry,
  WalterBetriebskostenrechnungEntry,
  WalterErhaltungsaufwendungEntry,
  WalterSelectionEntry,
  WalterUmlageEntry,
  WalterVertragEntry,
  WalterZaehlerEntry
} from '$WalterLib';
import { WalterApiHandler } from './WalterApiHandler';

export class WalterWohnungEntry extends WalterApiHandler {
  public static ApiURL: string = `/api/wohnungen`;

  constructor(
    public adresse: WalterAdresseEntry,
    public id: number,
    public bezeichnung: string,
    public wohnflaeche: number,
    public nutzflaeche: number,
    public einheiten: number,
    public notiz: string,
    public besitzer: WalterSelectionEntry | undefined,
    public haus: WalterWohnungEntry[],
    public zaehler: WalterZaehlerEntry[],
    public vertraege: WalterVertragEntry[],
    public betriebskostenrechnungen: WalterBetriebskostenrechnungEntry[],
    public erhaltungsaufwendungen: WalterErhaltungsaufwendungEntry[],
    public umlagen: WalterUmlageEntry[],
    public bewohner: string
  ) {
    super();
  }

  static fromJson(json: any) {
    const adresse = json.adresse && WalterAdresseEntry.fromJson(json.adresse);
    const besitzer =
      json.besitzer && WalterSelectionEntry.fromJson(json.besitzer);
    const haus = json.haus?.map(WalterWohnungEntry.fromJson);
    const zaehler = json.zaehler?.map(WalterZaehlerEntry.fromJson);
    const vertraege = json.vertraege?.map(WalterVertragEntry.fromJson);
    const betriebskostenrechnungen = json.betriebskostenrechnungen?.map(
      WalterBetriebskostenrechnungEntry.fromJson
    );
    const erhaltungsaufwendungen = json.erhaltungsaufwendungen?.map(
      WalterErhaltungsaufwendungEntry.fromJson
    );
    const umlagen = json.umlagen?.map(WalterUmlageEntry.fromJson);

    return new WalterWohnungEntry(
      adresse,
      json.id,
      json.bezeichnung,
      json.wohnflaeche,
      json.nutzflaeche,
      json.einheiten,
      json.notiz,
      besitzer,
      haus,
      zaehler,
      vertraege,
      betriebskostenrechnungen,
      erhaltungsaufwendungen,
      umlagen,
      json.bewohner
    );
  }
}
