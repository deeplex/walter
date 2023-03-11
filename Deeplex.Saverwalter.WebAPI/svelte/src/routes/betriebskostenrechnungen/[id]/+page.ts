import { walter_get } from "$WalterServices/requests";
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
                betriebskostentypen: walter_get('/api/selection/betriebskostentypen', fetch) as Promise<WalterSelectionEntry[]>,
                umlagen: walter_get('/api/selection/umlagen', fetch) as Promise<WalterSelectionEntry[]>,
                kontakte: walter_get('/api/selection/kontakte', fetch) as Promise<WalterSelectionEntry[]>,

                anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
        }
}