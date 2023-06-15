import { walter_selection } from '$walter/services/requests';
import { WalterBetriebskostenrechnungEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/betriebskostenrechnungen`;
    return {
        apiURL: apiURL,
        rows: WalterBetriebskostenrechnungEntry.GetAll<WalterBetriebskostenrechnungEntry>(
            fetch
        ),

        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen_wohnungen: walter_selection.umlagen_wohnungen(fetch)
    };
};
