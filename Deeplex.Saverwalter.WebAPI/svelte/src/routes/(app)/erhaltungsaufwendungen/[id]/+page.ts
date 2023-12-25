import { WalterErhaltungsaufwendungEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterErhaltungsaufwendungEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.erhaltungsaufwendung(params.id);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry: WalterErhaltungsaufwendungEntry.GetOne<WalterErhaltungsaufwendungEntry>(
            params.id,
            fetch
        )
    };
};
