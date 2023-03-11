import { walter_get, walter_selection } from "$WalterServices/requests";
import type { WalterSelectionEntry, WalterWohnungEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/wohnungen`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterWohnungEntry[]>,

        kontakte: walter_selection.kontakte(fetch),
    }
}