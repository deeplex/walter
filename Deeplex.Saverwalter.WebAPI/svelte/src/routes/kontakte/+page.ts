import { walter_get, walter_selection } from "$WalterServices/requests";
import type { WalterPersonEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/kontakte`;
    return {
        apiURL: apiURL,
        juristischePersonen: walter_selection.juristischePersonen(fetch),
        rows: walter_get(apiURL, fetch) as Promise<WalterPersonEntry[]>
    }
}