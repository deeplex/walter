import { walter_get, walter_selection } from "$WalterServices/requests";
import type { WalterAnhangEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/anhaenge/${params.id}`;
    return {
        id: params.id,
        apiURL: apiURL,
        a: walter_get(apiURL, fetch) as Promise<WalterAnhangEntry>,
        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen: walter_selection.umlagen(fetch),
        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        umlageschluessel: walter_selection.umlageschluessel(fetch),
        zaehler: walter_selection.zaehler(fetch),
        zaehlertypen: walter_selection.zaehlertypen(fetch),
    }
}