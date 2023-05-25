import { walter_selection } from '$WalterServices/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        apiURL: `/api/wohnungen`,
        title: 'Neue Wohnung',

        kontakte: walter_selection.kontakte(fetch)
    };
};
