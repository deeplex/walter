import { WalterVertragEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterVertragEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.vertrag(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterVertragEntry.GetOne<WalterVertragEntry>(params.id, fetch)
    };
};
