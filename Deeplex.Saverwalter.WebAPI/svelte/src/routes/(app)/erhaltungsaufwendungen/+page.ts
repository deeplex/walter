import { WalterErhaltungsaufwendungEntry } from '$WalterLib';
import { walter_selection } from '$WalterServices/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
  const apiURL = `/api/erhaltungsaufwendungen`;
  return {
    apiURL: apiURL,
    rows: WalterErhaltungsaufwendungEntry.GetAll<WalterErhaltungsaufwendungEntry>(
      fetch
    ),

    kontakte: walter_selection.kontakte(fetch),
    wohnungen: walter_selection.wohnungen(fetch)
  };
};
