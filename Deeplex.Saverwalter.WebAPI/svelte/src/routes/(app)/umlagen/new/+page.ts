import { walter_selection } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        apiURL: `/api/umlagen`,
        title: 'Neue Umlage',

        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen: walter_selection.umlagen(fetch),
        umlageschluessel: walter_selection.umlageschluessel(fetch),
        wohnungen: walter_selection.wohnungen(fetch),
        zaehler: walter_selection.zaehler(fetch)
    };
};
