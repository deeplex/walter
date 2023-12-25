import { WalterMieteEntry, WalterVertragEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterMieteEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.miete(params.id);
    const entry = WalterMieteEntry.GetOne<WalterMieteEntry>(params.id, fetch);

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        fileURL: fileUrl,
        entry,
        vertrag: entry.then((res) =>
            WalterVertragEntry.GetOne<WalterVertragEntry>(
                res.vertrag.id.toString(),
                fetch
            )
        )
    };
};
