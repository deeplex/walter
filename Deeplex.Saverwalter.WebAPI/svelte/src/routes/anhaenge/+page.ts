import { walter_get } from "$WalterServices/requests";
import type { WalterAnhangEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/anhaenge`;
    return {
        apiURL: apiURL,
        rows: walter_get(apiURL, fetch) as Promise<WalterAnhangEntry[]>
    }
}