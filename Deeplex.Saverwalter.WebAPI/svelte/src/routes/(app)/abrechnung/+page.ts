import { walter_selection } from '$walter/services/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        vertraege: walter_selection.vertraege(fetch)
    };
};