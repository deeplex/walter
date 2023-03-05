import { walter_get } from "$WalterServices/requests";
import type { WalterZaehlerEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/zaehler/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterZaehlerEntry>
    }
}