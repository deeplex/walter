import { WalterUmlageEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        rows: WalterUmlageEntry.GetAll<WalterUmlageEntry>(fetch)
    };
};
