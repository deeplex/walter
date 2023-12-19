import { WalterAdresseEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterAdresseEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.adresse(params.id);
    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterAdresseEntry.GetOne<WalterAdresseEntry>(params.id, fetch)
    };
};
