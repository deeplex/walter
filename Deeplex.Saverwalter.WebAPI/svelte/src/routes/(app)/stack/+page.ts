import { fileURL, walter_get_files } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        fileURL: fileURL.stack,
        files: await walter_get_files(fileURL.stack, fetch)
    };
};
