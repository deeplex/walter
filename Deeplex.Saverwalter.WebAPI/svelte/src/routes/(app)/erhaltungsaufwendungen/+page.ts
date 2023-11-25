import { WalterErhaltungsaufwendungEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        rows: WalterErhaltungsaufwendungEntry.GetAll<WalterErhaltungsaufwendungEntry>(
            fetch
        )
    };
};
