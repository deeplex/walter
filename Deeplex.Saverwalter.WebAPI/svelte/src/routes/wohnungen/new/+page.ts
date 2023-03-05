import { walter_get } from "$WalterServices/requests";
import type { WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        url: `/api/wohnungen`,
        title: 'Neue Wohnung',
        kontakte: walter_get('/api/selection/kontakte', fetch) as Promise<WalterSelectionEntry[]>,
    }
}