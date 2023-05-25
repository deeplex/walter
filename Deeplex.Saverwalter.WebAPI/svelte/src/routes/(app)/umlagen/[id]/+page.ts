import { WalterUmlageEntry } from '$walter/lib';
import { walter_selection } from '$walter/services/requests';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/umlagen/${params.id}`;
    const S3URL = `umlagen/${params.id}`;

    return {
        fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        a: WalterUmlageEntry.GetOne<WalterUmlageEntry>(params.id, fetch),
        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen: walter_selection.umlagen(fetch),
        umlageschluessel: walter_selection.umlageschluessel(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        kontakte: walter_selection.kontakte(fetch),
        zaehler: walter_selection.zaehler(fetch),
        zaehlertypen: walter_selection.zaehlertypen(fetch),

        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};
