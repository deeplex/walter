import { walter_selection } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        apiURL: `/api/erhaltungsaufwendungen`,
        title: 'Neue Erhaltungsaufwendung',

        kontakte: walter_selection.kontakte(fetch),
        wohnungen: walter_selection.wohnungen(fetch)
    };
};
