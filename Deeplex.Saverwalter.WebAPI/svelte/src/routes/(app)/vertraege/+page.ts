import { WalterVertragEntry } from '$walter/lib';
import { walter_selection } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/vertraege`;
    return {
        apiURL: apiURL,
        rows: WalterVertragEntry.GetAll<WalterVertragEntry>(fetch),

        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch)
    };
};
