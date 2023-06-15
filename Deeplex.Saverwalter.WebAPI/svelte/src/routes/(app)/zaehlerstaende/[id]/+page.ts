import { WalterZaehlerstandEntry } from '$walter/lib';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/zaehlerstaende/${params.id}`;
    const S3URL = `zaehlerstaende/${params.id}`;

    return {
        fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        entry: WalterZaehlerstandEntry.GetOne<WalterZaehlerstandEntry>(
            params.id,
            fetch
        ),
        anhaenge: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};
