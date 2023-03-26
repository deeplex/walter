import { WalterErhaltungsaufwendungEntry } from "$WalterLib";
import { walter_get, walter_selection } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterS3File } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const apiURL = `/api/erhaltungsaufwendungen/${params.id}`;
        const S3URL = `erhaltungsaufwendungen/${params.id}`;

        return {
                id: params.id,
                apiURL: apiURL,
                S3URL: S3URL,
                a: WalterErhaltungsaufwendungEntry.GetOne<WalterErhaltungsaufwendungEntry>(params.id, fetch),

                kontakte: walter_selection.kontakte(fetch),
                wohnungen: walter_selection.wohnungen(fetch),

                anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
        }
}