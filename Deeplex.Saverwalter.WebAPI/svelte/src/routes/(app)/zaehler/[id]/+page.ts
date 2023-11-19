import { WalterZaehlerEntry } from '$walter/lib';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/zaehler/${params.id}`;
    const S3URL = `zaehler/${params.id}`;

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        entry: WalterZaehlerEntry.GetOne<WalterZaehlerEntry>(params.id, fetch),

        files: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};
