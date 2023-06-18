import { WalterVertragVersionEntry } from '$walter/lib';
import { walter_s3_get_files } from '$walter/services/s3';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/vertragversionen/${params.id}`;
    const S3URL = `vertragversionen/${params.id}`;

    const entry = WalterVertragVersionEntry.GetOne<WalterVertragVersionEntry>(
        params.id,
        fetch
    )

    return {
        fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        entry,

        files: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>,
        refFiles: walter_s3_get_files(`vertraege/${(await entry).vertrag.id}`, fetch)
    };
};
