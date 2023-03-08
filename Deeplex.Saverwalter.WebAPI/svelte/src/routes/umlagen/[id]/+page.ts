import { walter_get } from "$WalterServices/requests";
import { get_files_with_common_prefix } from "$WalterServices/s3";
import type { WalterSelectionEntry, WalterUmlageEntry } from "$WalterTypes";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const url = `/api/umlagen/${params.id}`;
    return {
        id: params.id,
        url: url,
        a: walter_get(url, fetch) as Promise<WalterUmlageEntry>,
        betriebskostentypen: walter_get('/api/selection/betriebskostentypen', fetch) as Promise<WalterSelectionEntry[]>,
        umlagen: walter_get('/api/selection/umlagen', fetch) as Promise<WalterSelectionEntry[]>,
        umlageschluessel: walter_get('/api/selection/umlageschluessel', fetch) as Promise<WalterSelectionEntry[]>,
        wohnungen: walter_get('/api/selection/wohnungen', fetch) as Promise<WalterSelectionEntry[]>,
        kontakte: walter_get('/api/selection/kontakte', fetch) as Promise<WalterSelectionEntry[]>,

        anhaenge: get_files_with_common_prefix(`umlagen/${params.id}`, fetch) as Promise<string[]>
    }
}