import { WalterMieteEntry, WalterVertragEntry } from '$walter/lib';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/mieten/${params.id}`;
    const S3URL = `mieten/${params.id}`;
    const entry = WalterMieteEntry.GetOne<WalterMieteEntry>(params.id, fetch);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        entry,
        files: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>,
        vertrag: entry.then((res) =>
            WalterVertragEntry.GetOne<WalterVertragEntry>(
                res.vertrag.id.toString(),
                fetch
            )
        )
    };
};
