import { WalterKontaktEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterKontaktEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.kontakt(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterKontaktEntry.GetOne<WalterKontaktEntry>(params.id, fetch)
    };
};
