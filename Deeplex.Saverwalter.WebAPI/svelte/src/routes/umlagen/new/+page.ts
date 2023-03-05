import { walter_get } from "$WalterServices/requests";
import type { WalterSelectionEntry, WalterUmlageEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        url: `/api/umlagen`,
        title: 'Neue Umlage',
        betriebskostentypen: walter_get('/api/selection/betriebskostentypen', fetch) as Promise<WalterSelectionEntry[]>,
        umlagen: walter_get('/api/selection/umlagen', fetch) as Promise<WalterSelectionEntry[]>,
        umlageschluessel: walter_get('/api/selection/umlageschluessel', fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get('/api/selection/wohnungen', fetch) as Promise<WalterSelectionEntry[]>,
    }
}