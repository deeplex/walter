import { WalterUmlageEntry } from '$walter/lib';
import { S3URL, walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterUmlageEntry.ApiURL}/${params.id}`;
    const s3URL = S3URL.umlage(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: s3URL,
        entry: WalterUmlageEntry.GetOne<WalterUmlageEntry>(params.id, fetch)
    };
};
