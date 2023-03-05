import { walter_get } from "$WalterServices/requests";
import type { WalterWohnungEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/wohnungen`;
    return {
        url: url,
        rows: walter_get(url, fetch) as Promise<WalterWohnungEntry[]>
    }
}