import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterS3File, WalterSelectionEntry, WalterZaehlerEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const apiURL = `/api/zaehler/${params.id}`;
        const S3URL = `zaehler/${params.id}`;

        return {
                id: params.id,
                apiURL: apiURL,
                S3URL: S3URL,
                a: walter_get(apiURL, fetch) as Promise<WalterZaehlerEntry>,
                wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,
                zaehler: walter_get(`/api/selection/zaehler`, fetch) as Promise<WalterSelectionEntry[]>,
                zaehlertypen: walter_get(`/api/selection/zaehlertypen`, fetch) as Promise<WalterSelectionEntry[]>,

                anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
        }
}