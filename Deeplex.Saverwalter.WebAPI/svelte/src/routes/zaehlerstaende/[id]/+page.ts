import { walter_get } from "$WalterServices/requests";
import { get_files_with_common_prefix } from "$WalterServices/s3";
import type { WalterS3File, WalterZaehlerstandEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/zaehlerstaende/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterZaehlerstandEntry>,
        anhaenge: get_files_with_common_prefix(`zaehlerstaende/${params.id}`, fetch) as Promise<WalterS3File[]>
    }
}