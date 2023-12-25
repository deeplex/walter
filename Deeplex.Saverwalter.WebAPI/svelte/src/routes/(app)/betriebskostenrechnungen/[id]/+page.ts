import { WalterBetriebskostenrechnungEntry } from '$walter/lib';
import { fileURL } from '$walter/services/files';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterBetriebskostenrechnungEntry.ApiURL}/${params.id}`;
    const fileUrl = fileURL.betriebskostenrechnung(params.id);
    const entry =
        WalterBetriebskostenrechnungEntry.GetOne<WalterBetriebskostenrechnungEntry>(
            params.id,
            fetch
        );

    return {
        fetchImpl: fetch,
        id: params.id,
        apiURL: apiURL,
        filURL: fileUrl,
        entry
    };
};
