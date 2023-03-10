import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterS3File, WalterVertragEntry, WalterVertragVersionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const url = `/api/vertragversionen/${params.id}`;
        return {
                id: params.id,
                url: url,
                a: walter_get(url, fetch) as Promise<WalterVertragVersionEntry>,

                anhaenge: walter_s3_get_files(`vertragversion/${params.id}`, fetch) as Promise<WalterS3File[]>
        }
}