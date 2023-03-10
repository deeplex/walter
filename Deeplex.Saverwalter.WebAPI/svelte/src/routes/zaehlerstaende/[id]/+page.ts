import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterZaehlerstandEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/zaehlerstaende/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterZaehlerstandEntry>,
        anhaenge: walter_s3_get_files(`zaehlerstaende/${params.id}`, fetch) as Promise<string[]>
    }
}