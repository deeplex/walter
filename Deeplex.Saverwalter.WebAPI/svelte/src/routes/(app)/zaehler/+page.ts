import { WalterZaehlerEntry } from '$WalterLib';
import { walter_selection } from '$WalterServices/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
  const apiURL = `/api/zaehler`;
  return {
    apiURL: apiURL,
    rows: WalterZaehlerEntry.GetAll<WalterZaehlerEntry>(fetch),

    wohnungen: walter_selection.wohnungen(fetch),
    zaehler: walter_selection.zaehler(fetch),
    zaehlertypen: walter_selection.zaehlertypen(fetch)
  };
};
