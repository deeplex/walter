import { walter_get } from "$WalterServices/requests";
import type { WalterSelectionEntry, WalterZaehlerEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        url: `/api/zaehler`,
        title: 'Neuer Zähler',
        wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,
        zaehler: walter_get(`/api/selection/zaehler`, fetch) as Promise<WalterSelectionEntry[]>,
        zaehlertypen: walter_get(`/api/selection/zaehlertypen`, fetch) as Promise<WalterSelectionEntry[]>,
    }
}