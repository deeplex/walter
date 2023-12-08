import { WalterAccountEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        accounts: WalterAccountEntry.GetAll<WalterAccountEntry>(fetch)
    };
};