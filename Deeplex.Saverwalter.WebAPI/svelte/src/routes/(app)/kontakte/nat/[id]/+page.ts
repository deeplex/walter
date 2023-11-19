import { WalterNatuerlichePersonEntry } from '$walter/lib';
import type { WalterS3File } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/kontakte/nat/${params.id}`;
    const S3URL = `kontakte/nat/${params.id}`;

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        S3URL: S3URL,
        entry: WalterNatuerlichePersonEntry.GetOne<WalterNatuerlichePersonEntry>(
            `nat/${params.id}`,
            fetch
        ),

        files: walter_s3_get_files(S3URL, fetch) as Promise<WalterS3File[]>
    };
};
