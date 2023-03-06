import { walter_get } from "$WalterServices/requests";
import type { WalterBetriebskostenrechnungEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/betriebskostenrechnungen`;
    return {
        url: url,
        rows: walter_get(url, fetch) as Promise<WalterBetriebskostenrechnungEntry[]>,
        betriebskostentypen: walter_get(`/api/selection/betriebskostentypen`, fetch) as Promise<WalterSelectionEntry[]>,
        umlagen: walter_get(`/api/selection/umlagen`, fetch) as Promise<WalterSelectionEntry[]>,
    }
}