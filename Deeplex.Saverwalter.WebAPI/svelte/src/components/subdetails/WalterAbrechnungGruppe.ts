import { walter_goto } from '$walter/services/utils';
import type { WalterRechnungEntry } from '$walter/types/WalterBetriebskostenabrechnung.type';

export function goto_or_create_rechnung(
    punkt: WalterRechnungEntry,
    year: number
) {
    if (punkt.rechnungId) {
        walter_goto(`/betriebskostenrechnungen/${punkt.rechnungId}`);
    } else {
        const searchParams = new URLSearchParams();
        searchParams.set('typ', `${punkt.typId}`)
        searchParams.set('umlage', `${punkt.id}`);
        searchParams.set('jahr', `${year}`);
        // TODO betrag

        walter_goto(`/betriebskostenrechnungen/new?${searchParams.toString()}`);
    }
}
