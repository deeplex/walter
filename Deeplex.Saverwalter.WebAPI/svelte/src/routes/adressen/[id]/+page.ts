import { walter_get } from "$WalterServices/requests";
import type { WalterAdresseEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/adressen/${params.id}`;
    return {
        id: params.id,
        apiURL: apiURL,
        kontakte: walter_get(`/api/selection/kontakte`, fetch) as Promise<WalterSelectionEntry[]>,
        a: walter_get(apiURL, fetch) as Promise<WalterAdresseEntry>
    }
}