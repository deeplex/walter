import { WalterVertragVersionEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterVertragVersionEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.vertragversion(params.id);

    const entry = WalterVertragVersionEntry.GetOne<WalterVertragVersionEntry>(
        params.id,
        fetch
    );

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry
    };
};
