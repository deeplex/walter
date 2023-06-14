import { walter_s3_get_files } from '$walter/services/s3';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const S3URL = 'stack'

    return {
        S3URL,
        files: walter_s3_get_files('stack', fetch),
    };
};
