import { WalterWohnungEntry } from '$WalterLib';
import { walter_selection } from '$WalterServices/requests';
import { walter_s3_get_files } from '$WalterServices/s3';
import type { WalterS3File } from '$WalterTypes';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/wohnungen/${params.id}`;
    const S3URL = `wohnungen/${params.id}`;

    return {
        fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        a: WalterWohnungEntry.GetOne<WalterWohnungEntry>(params.id, fetch),

        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen: walter_selection.umlagen(fetch),
        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        zaehler: walter_selection.zaehler(fetch),
        zaehlertypen: walter_selection.zaehlertypen(fetch),
        umlageschluessel: walter_selection.umlageschluessel(fetch),

        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};
