import { walter_get } from "$WalterServices/requests";
import { walter_s3_get_files } from "$WalterServices/s3";
import type { WalterBetriebskostenrechnungEntry, WalterS3File, WalterSelectionEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
        const url = `/api/betriebskostenrechnungen/${params.id}`;
        return {
                id: params.id,
                url: url,
                a: walter_get(url, fetch) as Promise<WalterBetriebskostenrechnungEntry>,
                betriebskostentypen: walter_get('/api/selection/betriebskostentypen', fetch) as Promise<WalterSelectionEntry[]>,
                umlagen: walter_get('/api/selection/umlagen', fetch) as Promise<WalterSelectionEntry[]>,
                kontakte: walter_get('/api/selection/kontakte', fetch) as Promise<WalterSelectionEntry[]>,

                anhaenge: walter_s3_get_files(`betriebskostenrechnungen/${params.id}`, fetch) as Promise<WalterS3File[]>
        }
}