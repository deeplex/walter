import { WalterUmlageEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        apiURL: `${WalterUmlageEntry.ApiURL}`,
        title: 'Neue Umlage'
    };
};
