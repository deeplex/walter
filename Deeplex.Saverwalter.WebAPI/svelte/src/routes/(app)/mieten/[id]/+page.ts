import { WalterMieteEntry, WalterVertragEntry } from '$walter/lib';
import { S3URL, walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterMieteEntry.ApiURL}/${params.id}`;
    const s3URL = S3URL.miete(params.id);
    const entry = WalterMieteEntry.GetOne<WalterMieteEntry>(params.id, fetch);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: s3URL,
        entry,
        vertrag: entry.then((res) =>
            WalterVertragEntry.GetOne<WalterVertragEntry>(
                res.vertrag.id.toString(),
                fetch
            )
        )
    };
};
