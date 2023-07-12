import { walter_selection } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        apiURL: `/api/betriebskostenrechnungen`,
        title: 'Neue Betriebskostenrechnung'
    };
};
