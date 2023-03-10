import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterJuristischePersonEntry, WalterS3File, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/kontakte/jur/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterJuristischePersonEntry>,
        kontakte: walter_get(`/api/selection/kontakte`, fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,

        anhaenge: walter_s3_get_files(`kontakte/jur/${params.id}`, fetch) as Promise<WalterS3File[]>
    }
}