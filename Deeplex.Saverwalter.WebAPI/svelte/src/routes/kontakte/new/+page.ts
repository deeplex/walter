import { walter_selection } from "$WalterServices/requests";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {

    return {
        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        juristischePersonen: walter_selection.juristischePersonen(fetch),
    }
}