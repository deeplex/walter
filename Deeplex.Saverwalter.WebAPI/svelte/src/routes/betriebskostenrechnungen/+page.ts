import { walter_get, walter_selection } from "$WalterServices/requests";
import type { WalterBetriebskostenrechnungEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/betriebskostenrechnungen`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterBetriebskostenrechnungEntry[]>,

        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen: walter_selection.umlagen(fetch)
    }
}