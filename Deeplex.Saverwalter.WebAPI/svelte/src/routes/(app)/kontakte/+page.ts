import { WalterPersonEntry } from '$walter/lib';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        rows: WalterPersonEntry.GetAll<WalterPersonEntry>(fetch)
    };
};
