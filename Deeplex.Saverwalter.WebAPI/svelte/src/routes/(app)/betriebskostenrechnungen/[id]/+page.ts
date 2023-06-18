import { WalterBetriebskostenrechnungEntry } from '$walter/lib';
import { walter_selection } from '$walter/services/requests';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/betriebskostenrechnungen/${params.id}`;
    const S3URL = `betriebskostenrechnungen/${params.id}`;
    const entry = WalterBetriebskostenrechnungEntry.GetOne<WalterBetriebskostenrechnungEntry>(
        params.id,
        fetch
    );

    return {
        fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        entry,
        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen_wohnungen: walter_selection.umlagen_wohnungen(fetch),
        kontakte: walter_selection.kontakte(fetch),

        files: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>,
        refFiles: walter_s3_get_files(`umlagen/${(await entry).umlage.id}`, fetch)
    };
};
