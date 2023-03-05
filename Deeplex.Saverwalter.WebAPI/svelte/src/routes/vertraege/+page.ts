import { walter_get } from "$WalterServices/requests";
import type { WalterUmlageEntry, WalterVertragEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/vertraege`;
    return {
        url: url,
        rows: walter_get(url, fetch) as Promise<WalterVertragEntry[]>
    }
}