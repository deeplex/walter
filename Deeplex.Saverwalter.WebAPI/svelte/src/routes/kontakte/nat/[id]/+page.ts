import { walter_get } from "$WalterServices/requests";
import { get_files_with_common_prefix } from "$WalterServices/s3";
import type { WalterNatuerlichePersonEntry, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/kontakte/nat/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterNatuerlichePersonEntry>,
        kontakte: walter_get(`/api/selection/kontakte`, fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,

        anhaenge: get_files_with_common_prefix(`kontakte/nat/${params.id}`, fetch) as Promise<string[]>
    }
}