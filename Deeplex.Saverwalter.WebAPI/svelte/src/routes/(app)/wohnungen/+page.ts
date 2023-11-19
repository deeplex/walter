import { WalterWohnungEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/wohnungen`;
    return {
        fetchImpl: fetch,
        apiURL: apiURL,
        rows: WalterWohnungEntry.GetAll<WalterWohnungEntry>(fetch)
    };
};
