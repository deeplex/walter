import { WalterApiHandler } from './WalterApiHandler';
import { WalterSelectionEntry } from './WalterSelection';

export class WalterVertragVersionEntry extends WalterApiHandler {
  public static ApiURL = `/api/vertragversionen`;

  constructor(
    public id: number,
    public beginn: string,
    public personenzahl: number,
    public notiz: string,
    public grundmiete: number,
    public vertrag: WalterSelectionEntry
  ) {
    super();
  }

  static fromJson(json: any) {
    const vertrag = json.vertrag && WalterSelectionEntry.fromJson(json.vertrag);

    return new WalterVertragVersionEntry(
      json.id,
      json.beginn,
      json.personenzahl,
      json.notiz,
      json.grundmiete,
      vertrag
    );
  }
}
