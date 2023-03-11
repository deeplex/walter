import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterAdresseEntry, WalterS3File, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/adressen/${params.id}`;
    const S3URL = `adressen/${params.id}`;
    return {
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        kontakte: walter_get(`/api/selection/kontakte`, fetch) as Promise<WalterSelectionEntry[]>,
        a: walter_get(apiURL, fetch) as Promise<WalterAdresseEntry>,

        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    }
}