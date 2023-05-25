<script lang="ts">
    import { goto } from '$app/navigation';
    import { WalterDataTable } from '$WalterComponents';
    import type { WalterBetriebskostenabrechnungKostenpunkt } from '$WalterTypes';

    export let rows: WalterBetriebskostenabrechnungKostenpunkt[];
    export let year: number;

    const headers = [
        { key: 'typ.text', value: 'Kostenanteil' },
        { key: 'schluessel.text', value: 'Schlüssel' },
        { key: 'nutzungsintervall', value: 'Nutzungsintervall' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'anteil', value: 'Ihr Anteil' },
        { key: 'kosten', value: 'Ihre Kosten' }
    ];

    const navigate = (e: any) => {
        const punkt = e.detail as WalterBetriebskostenabrechnungKostenpunkt;

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
    };
</script>

<WalterDataTable {navigate} {headers} {rows} />
