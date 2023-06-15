import { WalterAdresseEntry } from '$walter/lib';
import { walter_selection } from '$walter/services/requests';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/adressen/${params.id}`;
    const S3URL = `adressen/${params.id}`;
    return {
        fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        zaehlertypen: walter_selection.zaehlertypen(fetch),
        umlagen: walter_selection.umlagen(fetch),
        entry: WalterAdresseEntry.GetOne<WalterAdresseEntry>(params.id, fetch),

        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};
