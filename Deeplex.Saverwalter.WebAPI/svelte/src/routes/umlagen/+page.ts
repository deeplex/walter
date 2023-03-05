import { walter_get } from "$WalterServices/requests";
import type { WalterSelectionEntry, WalterUmlageEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/umlagen`;
    return {
        url: url,
        rows: walter_get(url, fetch) as Promise<WalterUmlageEntry[]>,
        umlageschluessel: walter_get(`/api/selection/umlageschluessel`, fetch) as Promise<WalterSelectionEntry[]>,
        betriebskostentypen: walter_get(`/api/selection/betriebskostentypen`, fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,
    }
}
