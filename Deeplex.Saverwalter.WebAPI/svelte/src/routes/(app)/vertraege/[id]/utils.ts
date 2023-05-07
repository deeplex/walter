import type { WalterVertragVersionEntry } from '$WalterLib';
import { convertDate } from '$WalterServices/utils';

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
