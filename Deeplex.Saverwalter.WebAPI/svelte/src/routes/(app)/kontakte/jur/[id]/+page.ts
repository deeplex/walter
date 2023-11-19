import { WalterJuristischePersonEntry } from '$walter/lib';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/kontakte/jur/${params.id}`;
    const S3URL = `kontakte/jur/${params.id}`;

    return {
        fetchImpl: fetch,
        id: params.id,
        S3URL: S3URL,
        apiURL: apiURL,
        entry: WalterJuristischePersonEntry.GetOne<WalterJuristischePersonEntry>(
            `jur/${params.id}`,
            fetch
        ),

        files: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};
