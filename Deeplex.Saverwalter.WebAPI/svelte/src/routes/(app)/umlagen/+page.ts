import { WalterUmlageEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/umlagen`;
    return {
        fetchImpl: fetch,
        apiURL: apiURL,
        rows: WalterUmlageEntry.GetAll<WalterUmlageEntry>(fetch)
    };
};
