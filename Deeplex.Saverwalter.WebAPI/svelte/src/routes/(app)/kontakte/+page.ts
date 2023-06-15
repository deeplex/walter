import { WalterPersonEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/kontakte`;
    return {
        apiURL: apiURL,
        rows: WalterPersonEntry.GetAll<WalterPersonEntry>(fetch)
    };
};
