import { WalterMietminderungEntry } from '$walter/lib';
import { fileURL, walter_get_files } from '$walter/services/files';
import type { WalterFile } from '$walter/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterMietminderungEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.mietminderung(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterMietminderungEntry.GetOne<WalterMietminderungEntry>(
            params.id,
            fetch
        )
    };
};
