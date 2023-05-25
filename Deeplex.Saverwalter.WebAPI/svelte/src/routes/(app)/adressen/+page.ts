import { WalterAdresseEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        rows: WalterAdresseEntry.GetAll<WalterAdresseEntry>(fetch)
    };
};
