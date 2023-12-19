import { WalterUmlagetypEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterUmlagetypEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.umlagetyp(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterUmlagetypEntry.GetOne<WalterUmlagetypEntry>(
            params.id,
            fetch
        )
    };
};
