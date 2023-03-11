import { walter_get } from "$WalterServices/requests";
import type { WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        apiURL: `/api/erhaltungsaufwendungen`,
        title: 'Neue Erhaltungsaufwendung',
        kontakte: walter_get(`/api/selection/kontakte`, fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,
    }
}