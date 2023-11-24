import { WalterAccountEntry, WalterAdresseEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterAdresseEntry.ApiURL}/${params.id}`;

    return {
        apiURL,
        fetchImpl: fetch,
        entry: WalterAccountEntry.GetOne<WalterAccountEntry>(params.id, fetch)
    };
};
