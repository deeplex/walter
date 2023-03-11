import { walter_get, walter_selection } from "$WalterServices/requests";
import type { WalterSelectionEntry, WalterZaehlerEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/zaehler`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterZaehlerEntry[]>,

        wohnungen: walter_selection.wohnungen(fetch),
        zaehler: walter_selection.zaehler(fetch),
        zaehlertypen: walter_selection.zaehlertypen(fetch),
    }
}