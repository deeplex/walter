import { WalterWohnungEntry } from '$walter/lib';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/wohnungen/${params.id}`;
    const S3URL = `wohnungen/${params.id}`;

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        entry: WalterWohnungEntry.GetOne<WalterWohnungEntry>(params.id, fetch),

        files: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};
