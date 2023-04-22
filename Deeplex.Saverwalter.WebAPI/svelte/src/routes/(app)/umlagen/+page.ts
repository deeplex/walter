import { WalterUmlageEntry } from '$WalterLib';
import { walter_get, walter_selection } from '$WalterServices/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
  const apiURL = `/api/umlagen`;
  return {
    apiURL: apiURL,
    rows: WalterUmlageEntry.GetAll<WalterUmlageEntry>(fetch),

    umlageschluessel: walter_selection.umlageschluessel(fetch),
    betriebskostentypen: walter_selection.betriebskostentypen(fetch),
    wohnungen: walter_selection.wohnungen(fetch)
  };
};
