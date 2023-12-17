import { S3URL, walter_s3_get_files } from '$walter/services/s3';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        S3URL: S3URL.stack,
        files: await walter_s3_get_files(S3URL.stack, fetch)
    };
};
