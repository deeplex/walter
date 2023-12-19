import { WalterZaehlerstandEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterZaehlerstandEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.zaehlerstand(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterZaehlerstandEntry.GetOne<WalterZaehlerstandEntry>(
            params.id,
            fetch
        )
    };
};
