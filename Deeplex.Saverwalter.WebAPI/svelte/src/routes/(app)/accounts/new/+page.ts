import { WalterAccountEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        apiURL: `${WalterAccountEntry.ApiURL}`,
        title: 'Neuen Nutzer anlegen'
    };
};
