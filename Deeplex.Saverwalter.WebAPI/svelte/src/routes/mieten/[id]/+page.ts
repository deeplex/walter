import { walter_get } from "$WalterServices/requests";
import type { WalterMieteEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/mieten/${params.id}`;
    return {
        id: params.id,
        apiURL: apiURL,
        a: walter_get(apiURL, fetch) as Promise<WalterMieteEntry>
    }
}