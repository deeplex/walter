import type { WalterVertragVersionEntry } from '$WalterLib';
import { getKostenpunkt } from '$WalterServices/abrechnung';
import { walter_get } from '$WalterServices/requests';
import { convertDate } from '$WalterServices/utils';
import type {
  WalterBetriebskostenabrechnungEntry,
  WalterBetriebskostenabrechnungKostengruppenEntry
} from '$WalterTypes';

export function getMietminderungEntry(vertragId: string) {
  const today = new Date();
  return {
    vertrag: { id: vertragId, text: '' },
    beginn: convertDate(today),
    ende: convertDate(new Date(today.setMonth(today.getMonth() + 1)))
  };
}

export function getVertragversionEntry(
  vertragId: string,
  lastVersion: WalterVertragVersionEntry | undefined
) {
  return {
    vertrag: { id: vertragId, text: '' },
    beginn: convertDate(new Date()),
    personenzahl: lastVersion?.personenzahl,
    grundmiete: lastVersion?.grundmiete
  };
}

export function getMieteEntry(
  vertragId: string,
  lastVersion: WalterVertragVersionEntry | undefined
) {
  return {
    vertrag: { id: vertragId, text: '' },
    zahlungsdatum: convertDate(new Date()),
    betrag: lastVersion?.grundmiete || 0
  };
}

type fetchType = (
  input: RequestInfo | URL,
  init?: RequestInit | undefined
) => Promise<Response>;
export async function loadAbrechnung(
  vertragId: string,
  year: string,
  fetch: fetchType
) {
  const abrechnungURL = `/api/betriebskostenabrechnung/${vertragId}/${year}`;

  const a = await (walter_get(
    abrechnungURL,
    fetch
  ) as Promise<WalterBetriebskostenabrechnungEntry>);

  return {
    ...a,
    kostengruppen: getKostengruppen(a)
  } as WalterBetriebskostenabrechnungKostengruppenEntry;
}

export function getKostengruppen(
  abrechnung: WalterBetriebskostenabrechnungEntry
) {
  return abrechnung.gruppen.map((e) => {
    const kostenpunkte = e.umlagen.map((u, i) =>
      getKostenpunkt(
        i,
        u,
        new Date(abrechnung.nutzungsbeginn).toLocaleDateString('de-De'),
        new Date(abrechnung.nutzungsende).toLocaleDateString('de-De'),
        abrechnung.jahr,
        e.wfZeitanteil
      )
    );

    return {
      kostenpunkte,
      ...e
    };
  });
}
