import { walter_get } from "$WalterServices/requests";
import type { WalterSelectionEntry, WalterZaehlerEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/zaehler`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterZaehlerEntry[]>,
        wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,
        zaehler: walter_get(`/api/selection/zaehler`, fetch) as Promise<WalterSelectionEntry[]>,
        zaehlertypen: walter_get(`/api/selection/zaehlertypen`, fetch) as Promise<WalterSelectionEntry[]>,
    }
}