import { walter_selection } from '$WalterServices/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        apiURL: `/api/zaehler`,
        title: 'Neuer Zähler',

        wohnungen: walter_selection.wohnungen(fetch),
        umlagen: walter_selection.umlagen(fetch),
        zaehlertypen: walter_selection.zaehlertypen(fetch)
    };
};
