import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterSelectionEntry, WalterZaehlerEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/zaehler/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterZaehlerEntry>,
        wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,
        zaehler: walter_get(`/api/selection/zaehler`, fetch) as Promise<WalterSelectionEntry[]>,
        zaehlertypen: walter_get(`/api/selection/zaehlertypen`, fetch) as Promise<WalterSelectionEntry[]>,

        anhaenge: walter_s3_get_files(`zaehler/${params.id}`, fetch) as Promise<string[]>
    }
}