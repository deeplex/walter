import { walter_get, walter_selection } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterBetriebskostenrechnungEntry, WalterS3File, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const apiURL = `/api/betriebskostenrechnungen/${params.id}`;
        const S3URL = `betriebskostenrechnungen/${params.id}`;
        return {
                id: params.id,
                apiURL: apiURL,
                S3URL: S3URL,
                a: walter_get(apiURL, fetch) as Promise<WalterBetriebskostenrechnungEntry>,
                betriebskostentypen: walter_selection.betriebskostentypen(fetch),
                umlagen: walter_selection.umlagen(fetch),
                kontakte: walter_selection.kontakte(fetch),

                anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
        }
}