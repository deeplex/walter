import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterBetriebskostenabrechnungEntry, WalterS3File } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/betriebskostenabrechnung/${params.id}/${params.year}`;
    const S3URL = `vertraege/${params.id}`;

    return {
        id: params.id,
        year: params.year,
        apiURL: apiURL,
        S3URL: S3URL,
        a: walter_get(apiURL, fetch) as Promise<WalterBetriebskostenabrechnungEntry>,

        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    }
}