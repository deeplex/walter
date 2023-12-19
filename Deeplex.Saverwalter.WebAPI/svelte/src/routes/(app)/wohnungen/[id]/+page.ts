import { WalterWohnungEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterWohnungEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.wohnung(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterWohnungEntry.GetOne<WalterWohnungEntry>(params.id, fetch)
    };
};
