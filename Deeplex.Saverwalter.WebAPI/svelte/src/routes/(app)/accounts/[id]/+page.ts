import { WalterAccountEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `${WalterAccountEntry.ApiURL}/${params.id}`;

    const permissions = {
        read: true,
        update: true,
        remove: true
    };

    return {
        apiURL,
        fetchImpl: fetch,
        entry: {
            ...(await WalterAccountEntry.GetOne<WalterAccountEntry>(
                params.id,
                fetch
            )),
            permissions
        }
    };
};
