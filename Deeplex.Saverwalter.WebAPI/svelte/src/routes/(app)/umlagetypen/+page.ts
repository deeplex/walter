import { WalterUmlagetypEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/umlagetypen`;
    return {
        fetchImpl: fetch,
        apiURL: apiURL,
        rows: WalterUmlagetypEntry.GetAll<WalterUmlagetypEntry>(fetch)
    };
};
