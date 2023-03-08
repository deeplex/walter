import { walter_get } from "$WalterServices/requests";
import { get_files_with_common_prefix } from "$WalterServices/s3";
import type { WalterVertragEntry, WalterVertragVersionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/vertragversion/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterVertragVersionEntry>,

        anhaenge: get_files_with_common_prefix(`vertragversion/${params.id}`, fetch) as Promise<string[]>
    }
}