import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterS3File, WalterZaehlerstandEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const apiURL = `/api/zaehlerstaende/${params.id}`;
        const S3URL = `zaehlerstaende/${params.id}`;

        return {
                id: params.id,
                apiURL: apiURL,
                S3URL: S3URL,
                a: walter_get(apiURL, fetch) as Promise<WalterZaehlerstandEntry>,
                anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
        }
}