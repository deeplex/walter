import { walter_get } from "$WalterServices/requests";
import type { WalterPersonEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/kontakte`;
    return {
        url: url,
        rows: walter_get(url, fetch) as Promise<WalterPersonEntry[]>
    }
}