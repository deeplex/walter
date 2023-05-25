import { walter_selection } from '$walter/services/requests';
import { WalterWohnungEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/wohnungen`;
    return {
        apiURL: apiURL,
        rows: WalterWohnungEntry.GetAll<WalterWohnungEntry>(fetch),

        kontakte: walter_selection.kontakte(fetch)
    };
};
