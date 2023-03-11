import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterBetriebskostenrechnungEntry, WalterErhaltungsaufwendungEntry, WalterS3File, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const apiURL = `/api/erhaltungsaufwendungen/${params.id}`;
        return {
                id: params.id,
                apiURL: apiURL,
                a: walter_get(apiURL, fetch) as Promise<WalterErhaltungsaufwendungEntry>,
                kontakte: walter_get(`/api/selection/kontakte`, fetch) as Promise<WalterSelectionEntry[]>,
                wohnungen: walter_get(`/api/selection/wohnungen`, fetch) as Promise<WalterSelectionEntry[]>,

                anhaenge: walter_s3_get_files(`erhaltungsaufwendungen/${params.id}`, fetch) as Promise<WalterS3File[]>
        }
}