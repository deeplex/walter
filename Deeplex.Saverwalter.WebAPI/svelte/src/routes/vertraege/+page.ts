import { walter_get } from "$WalterServices/requests";
import type { WalterSelectionEntry, WalterUmlageEntry, WalterVertragEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/vertraege`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterVertragEntry[]>,
        kontakte: walter_get(`/api/selection/kontakte`, fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,
    }
}