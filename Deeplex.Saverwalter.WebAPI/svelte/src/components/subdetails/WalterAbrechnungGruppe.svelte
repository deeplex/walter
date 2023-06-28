<script lang="ts">
    import { WalterDataTable } from '$walter/components';
    import type { WalterBetriebskostenabrechnungKostenpunkt } from '$walter/types';
    import { goto_or_create_rechnung } from './WalterAbrechnungGruppe';

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
        goto_or_create_rechnung(
            e.detail as WalterBetriebskostenabrechnungKostenpunkt,
            year
        );
    };
</script>

<WalterDataTable {navigate} {headers} {rows} />
