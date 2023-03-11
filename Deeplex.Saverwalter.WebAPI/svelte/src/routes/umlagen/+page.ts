import { walter_get, walter_selection } from "$WalterServices/requests";
import type { WalterSelectionEntry, WalterUmlageEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/umlagen`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterUmlageEntry[]>,

        umlageschluessel: walter_selection.umlageschluessel(fetch),
        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
    }
}
