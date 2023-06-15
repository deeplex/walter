import { walter_selection } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        apiURL: `/api/betriebskostenrechnungen`,
        title: 'Neue Betriebskostenrechnung',
        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen_wohnungen: walter_selection.umlagen_wohnungen(fetch)
    };
};
