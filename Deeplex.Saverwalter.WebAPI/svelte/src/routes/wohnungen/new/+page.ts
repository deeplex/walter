import { walter_get, walter_selection } from "$WalterServices/requests";
import type { WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        apiURL: `/api/wohnungen`,
        title: 'Neue Wohnung',

        kontakte: walter_selection.kontakte(fetch),
    }
}