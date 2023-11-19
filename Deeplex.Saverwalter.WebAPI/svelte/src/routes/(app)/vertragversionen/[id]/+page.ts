import { WalterVertragVersionEntry } from '$walter/lib';
import { S3URL, walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterVertragVersionEntry.ApiURL}/${params.id}`;
    const s3URL = S3URL.vertragversion(params.id);

    const entry = WalterVertragVersionEntry.GetOne<WalterVertragVersionEntry>(
        params.id,
        fetch
    );

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: s3URL,
        entry,

        files: walter_s3_get_files(s3URL, fetch) as Promise<WalterS3File[]>
    };
};
