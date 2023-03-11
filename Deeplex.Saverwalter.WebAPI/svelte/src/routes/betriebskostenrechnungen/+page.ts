import { walter_get } from "$WalterServices/requests";
import type { WalterBetriebskostenrechnungEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/betriebskostenrechnungen`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterBetriebskostenrechnungEntry[]>,
        betriebskostentypen: walter_get(`/api/selection/betriebskostentypen`, fetch) as Promise<WalterSelectionEntry[]>,
        umlagen: walter_get(`/api/selection/umlagen`, fetch) as Promise<WalterSelectionEntry[]>,
    }
}