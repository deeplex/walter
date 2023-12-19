import { WalterZaehlerEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterZaehlerEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.zaehler(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterZaehlerEntry.GetOne<WalterZaehlerEntry>(params.id, fetch)
    };
};
