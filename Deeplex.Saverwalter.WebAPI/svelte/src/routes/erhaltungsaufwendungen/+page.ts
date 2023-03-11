import { walter_get, walter_selection } from "$WalterServices/requests";
import type { WalterErhaltungsaufwendungEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/erhaltungsaufwendungen`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterErhaltungsaufwendungEntry[]>,

        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
    }
}