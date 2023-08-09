<script lang="ts">
    import { WalterDataTable } from '$walter/components';
    import type { WalterRechnungEntry } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import { goto_or_create_rechnung } from './WalterAbrechnungGruppe';

    export let rows: WalterRechnungEntry[];
    export let year: number;

    const headers = [
        { key: 'typ', value: 'Kostenanteil' },
        { key: 'schluessel', value: 'Schlüssel' },
        { key: 'gesamtBetrag', value: 'Betrag' },
        { key: 'anteil', value: 'Anteil' },
        { key: 'betrag', value: 'Kosten' }
    ];

    const navigate = (e: any) => {
        goto_or_create_rechnung(e.detail, year);
    };
</script>

<WalterDataTable size="short" fullHeight {navigate} {headers} {rows} />
