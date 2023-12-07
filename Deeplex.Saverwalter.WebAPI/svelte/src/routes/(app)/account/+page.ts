import { WalterAccountEntry, WalterAdresseEntry } from '$walter/lib';
import { walter_get } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `${WalterAccountEntry.ApiURLUser}`;

    return {
        apiURL,
        fetchImpl: fetch,
        entry: (await walter_get(apiURL, fetch)) as WalterAccountEntry
    };
};
