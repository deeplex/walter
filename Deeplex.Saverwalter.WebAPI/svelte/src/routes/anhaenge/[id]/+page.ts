import { walter_get } from "$WalterServices/requests";
import type { WalterAnhangEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/anhaenge/${params.id}`;
    return {
        id: params.id,
        apiURL: apiURL,
        a: walter_get(apiURL, fetch) as Promise<WalterAnhangEntry>,
        betriebskostentypen: walter_get('/api/selection/betriebskostentypen', fetch) as Promise<WalterSelectionEntry[]>,
        umlagen: walter_get('/api/selection/umlagen', fetch) as Promise<WalterSelectionEntry[]>,
        kontakte: walter_get('/api/selection/kontakte', fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get('/api/selection/wohnungen', fetch) as Promise<WalterSelectionEntry[]>,
        umlageschluessel: walter_get('/api/selection/umlageschluessel', fetch) as Promise<WalterSelectionEntry[]>,
        zaehler: walter_get('/api/selection/zaehler', fetch) as Promise<WalterSelectionEntry[]>,
        zaehlertypen: walter_get('/api/selection/zaehlertypen', fetch) as Promise<WalterSelectionEntry[]>,
    }
}