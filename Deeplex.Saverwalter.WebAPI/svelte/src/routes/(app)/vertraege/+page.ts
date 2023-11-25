import { WalterVertragEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        rows: WalterVertragEntry.GetAll<WalterVertragEntry>(fetch)
    };
};
