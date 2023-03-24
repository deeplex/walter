import { walter_selection } from "$WalterServices/requests";
import { WalterBetriebskostenrechnungEntry } from "$WalterLib";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, fetch }) => {
    const apiURL = `/api/betriebskostenrechnungen`;
    return {
        apiURL: apiURL,
        rows: WalterBetriebskostenrechnungEntry.GetAll<WalterBetriebskostenrechnungEntry>(fetch),

        betriebskostentypen: walter_selection.betriebskostentypen(fetch),
        umlagen: walter_selection.umlagen(fetch)
    }
}