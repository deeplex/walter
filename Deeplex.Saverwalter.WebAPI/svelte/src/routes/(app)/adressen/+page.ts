import { WalterAdresseEntry } from '$WalterLib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        rows: WalterAdresseEntry.GetAll<WalterAdresseEntry>(fetch)
    };
};
