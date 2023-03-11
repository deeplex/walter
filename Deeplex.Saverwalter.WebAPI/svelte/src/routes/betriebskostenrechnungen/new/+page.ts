import { walter_get } from "$WalterServices/requests";
import type { WalterBetriebskostenrechnungEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        apiURL: `/api/betriebskostenrechnungen`,
        title: 'Neue Betriebskostenrechnung',
        betriebskostentypen: walter_get('/api/selection/betriebskostentypen', fetch) as Promise<WalterSelectionEntry[]>,
        umlagen: walter_get('/api/selection/umlagen', fetch) as Promise<WalterSelectionEntry[]>,
    }
}