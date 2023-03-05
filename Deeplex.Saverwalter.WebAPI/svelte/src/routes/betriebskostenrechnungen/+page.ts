import { walter_get } from "$WalterServices/requests";
import type { WalterBetriebskostenrechnungEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/betriebskostenrechnungen`;
    return {
        url: url,
        rows: walter_get(url, fetch) as Promise<WalterBetriebskostenrechnungEntry[]>
    }
}