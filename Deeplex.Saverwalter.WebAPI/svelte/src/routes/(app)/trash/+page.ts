import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const S3URL = 'trash';

    return {
        fetchImpl: fetch,
        S3URL
    };
};
