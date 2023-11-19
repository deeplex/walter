import { WalterBetriebskostenrechnungEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/betriebskostenrechnungen`;
    return {
        fetchImpl: fetch,
        apiURL: apiURL,
        rows: WalterBetriebskostenrechnungEntry.GetAll<WalterBetriebskostenrechnungEntry>(
            fetch
        )
    };
};
