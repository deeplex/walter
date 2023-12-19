import { WalterUmlageEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterUmlageEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.umlage(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURl: fileUrl,
        entry: WalterUmlageEntry.GetOne<WalterUmlageEntry>(params.id, fetch)
    };
};
