import { WalterVertragEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/vertraege`;
    return {
        fetchImpl: fetch,
        apiURL: apiURL,
        rows: WalterVertragEntry.GetAll<WalterVertragEntry>(fetch)
    };
};
