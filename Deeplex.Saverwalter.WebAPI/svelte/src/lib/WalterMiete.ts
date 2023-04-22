import { WalterSelectionEntry } from '$WalterLib';
import { WalterApiHandler } from './WalterApiHandler';

export class WalterMieteEntry extends WalterApiHandler {
  public static ApiURL: string = `/api/mieten`;

  constructor(
    public id: number,
    public betreffenderMonat: string,
    public zahlungsdatum: string,
    public betrag: number,
    public vertrag: WalterSelectionEntry,
    public notiz: string
  ) {
    super();
  }

  static fromJson(json: any) {
    const vertrag = json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);

    return new WalterMieteEntry(
      json.id,
      json.betreffenderMonat,
      json.zahlungsdatum,
      json.betrag,
      vertrag,
      json.notiz
    );
  }
}
