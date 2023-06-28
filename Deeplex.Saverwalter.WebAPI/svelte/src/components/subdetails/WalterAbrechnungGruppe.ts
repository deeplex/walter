import { goto } from '$app/navigation';
import type { WalterBetriebskostenabrechnungKostenpunkt } from '$walter/types';

export function goto_or_create_rechnung(
    punkt: WalterBetriebskostenabrechnungKostenpunkt,
    year: number
) {
    if (punkt.betriebskostenrechnungId) {
        goto(`/betriebskostenrechnungen/${punkt.betriebskostenrechnungId}`);
    } else {
        const searchParams = new URLSearchParams();
        searchParams.set('typ', `${punkt.typ.id}`);
        searchParams.set('umlage', `${punkt.umlageId}`);
        searchParams.set('jahr', `${year}`);
        // TODO betrag

        goto(`/betriebskostenrechnungen/new?${searchParams.toString()}`);
    }
}
