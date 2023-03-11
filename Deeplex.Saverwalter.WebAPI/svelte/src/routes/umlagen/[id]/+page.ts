import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterS3File, WalterSelectionEntry, WalterUmlageEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const apiURL = `/api/umlagen/${params.id}`;
        return {
                id: params.id,
                apiURL: apiURL,
                a: walter_get(apiURL, fetch) as Promise<WalterUmlageEntry>,
                betriebskostentypen: walter_get('/api/selection/betriebskostentypen', fetch) as Promise<WalterSelectionEntry[]>,
                umlagen: walter_get('/api/selection/umlagen', fetch) as Promise<WalterSelectionEntry[]>,
                umlageschluessel: walter_get('/api/selection/umlageschluessel', fetch) as Promise<WalterSelectionEntry[]>,
                wohnungen: walter_get('/api/selection/wohnungen', fetch) as Promise<WalterSelectionEntry[]>,
                kontakte: walter_get('/api/selection/kontakte', fetch) as Promise<WalterSelectionEntry[]>,

                anhaenge: walter_s3_get_files(`umlagen/${params.id}`, fetch) as Promise<WalterS3File[]>
        }
}