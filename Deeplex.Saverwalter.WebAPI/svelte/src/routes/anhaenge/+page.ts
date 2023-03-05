import { walter_get } from "$WalterServices/requests";
import type { WalterAnhangEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params }) => {
    const url = `/api/anhaenge`;
    return {
        url: url,
        rows: walter_get(url) as Promise<WalterAnhangEntry[]>
    }
}