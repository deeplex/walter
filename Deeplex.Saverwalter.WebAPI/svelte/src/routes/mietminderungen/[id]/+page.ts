import { walter_get } from "$WalterServices/requests";
import type { WalterMietminderungEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/mietminderungen/${params.id}`;
    return {
        id: params.id,
        apiURL: apiURL,
        a: walter_get(apiURL, fetch) as Promise<WalterMietminderungEntry>
    }
}