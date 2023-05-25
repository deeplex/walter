import { WalterPersonEntry } from '$WalterLib';
import { walter_selection } from '$WalterServices/requests';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    const apiURL = `/api/kontakte`;
    return {
        apiURL: apiURL,
        juristischePersonen: walter_selection.juristischePersonen(fetch),
        rows: WalterPersonEntry.GetAll<WalterPersonEntry>(fetch)
    };
};
