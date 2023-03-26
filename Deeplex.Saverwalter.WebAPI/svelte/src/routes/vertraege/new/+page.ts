import { walter_selection } from "$WalterServices/requests";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        apiURL: `/api/vertraege`,
        title: 'Neuer Vertrag',

        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
    }
}