import { walter_get } from "$WalterServices/requests";
import type { WalterErhaltungsaufwendungEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/erhaltungsaufwendungen`;
    return {
        url: url,
        rows: walter_get(url, fetch) as Promise<WalterErhaltungsaufwendungEntry[]>,
        kontakte: walter_get('/api/selection/kontakte', fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get('/api/selection/wohnungen', fetch) as Promise<WalterSelectionEntry[]>,
    }
}