import { walter_get, walter_selection } from "$WalterServices/requests";
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
        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        zaehlertypen: walter_selection.zaehlertypen(fetch),
        zaehler: walter_selection.zaehler(fetch),
        a: walter_get(apiURL, fetch) as Promise<WalterAdresseEntry>,

        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    }
}