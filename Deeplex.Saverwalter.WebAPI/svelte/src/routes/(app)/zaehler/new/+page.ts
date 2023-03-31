import { walter_selection } from "$WalterServices/requests";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        apiURL: `/api/zaehler`,
        title: 'Neuer Zähler',

        wohnungen: walter_selection.wohnungen(fetch),
        zaehler: walter_selection.zaehler(fetch),
        zaehlertypen: walter_selection.zaehlertypen(fetch),
    }
}