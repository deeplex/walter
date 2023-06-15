import { WalterUmlageEntry } from '$walter/lib';
import { walter_selection } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/umlagen`;
    return {
        apiURL: apiURL,
        rows: WalterUmlageEntry.GetAll<WalterUmlageEntry>(fetch),

        umlageschluessel: walter_selection.umlageschluessel(fetch),
        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        zaehler: walter_selection.zaehler(fetch)
    };
};
