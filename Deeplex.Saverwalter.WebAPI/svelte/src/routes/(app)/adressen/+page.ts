import { walter_get } from "$WalterServices/requests";
import { WalterAdresseEntry } from "$WalterLib";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    return {
        rows: WalterAdresseEntry.GetAll<WalterAdresseEntry>(fetch)
    }
}