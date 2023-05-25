import { walter_selection } from '$WalterServices/requests';
import { WalterWohnungEntry } from '$WalterLib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/wohnungen`;
    return {
        apiURL: apiURL,
        rows: WalterWohnungEntry.GetAll<WalterWohnungEntry>(fetch),

        kontakte: walter_selection.kontakte(fetch)
    };
};
