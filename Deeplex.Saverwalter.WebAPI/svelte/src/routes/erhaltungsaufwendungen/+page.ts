import { walter_get } from "$WalterServices/requests";
import type { WalterErhaltungsaufwendungEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params }) => {
    const url = `/api/erhaltungsaufwendungen`;
    return {
        url: url,
        rows: walter_get(url) as Promise<WalterErhaltungsaufwendungEntry[]>
    }
}