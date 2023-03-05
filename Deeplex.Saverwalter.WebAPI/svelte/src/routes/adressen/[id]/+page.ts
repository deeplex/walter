import { walter_get } from "$WalterServices/requests";
import type { WalterAdresseEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/adressen/${params.id}`;
    return {
        id: params.id,
        url: url,
        kontakte: walter_get(`/api/selection/kontakte`, fetch) as Promise<WalterSelectionEntry[]>,
        a: walter_get(url, fetch) as Promise<WalterAdresseEntry>
    }
}