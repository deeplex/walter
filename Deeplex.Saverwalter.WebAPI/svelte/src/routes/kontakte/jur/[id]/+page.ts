import { walter_get, walter_selection } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterJuristischePersonEntry, WalterS3File, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const apiURL = `/api/kontakte/jur/${params.id}`;
        const S3URL = `kontakte/jur/${params.id}`;

        return {
                id: params.id,
                S3URL: S3URL,
                apiURL: apiURL,
                a: walter_get(apiURL, fetch) as Promise<WalterJuristischePersonEntry>,

                kontakte: walter_selection.kontakte(fetch),
                wohnungen: walter_selection.wohnungen(fetch),
                juristischePersonen: walter_selection.juristischePersonen(fetch),

                anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
        }
}