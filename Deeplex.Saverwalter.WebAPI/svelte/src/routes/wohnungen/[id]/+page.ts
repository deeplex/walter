import { walter_get } from "$WalterServices/requests";
import type { WalterVertragEntry, WalterVertragVersionEntry, WalterWohnungEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/wohnungen/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterWohnungEntry>
    }
}