<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterBetriebskostenrechnung,
        WalterDataWrapper
    } from '$walter/components';
    import { WalterBetriebskostenrechnungEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    export let fullHeight = false;
    export let rows: WalterBetriebskostenrechnungEntry[];
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;

    const headers = [
        { key: 'typ.text', value: 'Typ' },
        { key: 'umlage.text', value: 'Wohnungen' },
        { key: 'betreffendesJahr', value: 'Betreffendes Jahr' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'datum', value: 'Datum' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.betriebskostenrechnung(e.detail.id);

    export let entry: Partial<WalterBetriebskostenrechnungEntry> | undefined =
        undefined;
</script>

<WalterDataWrapper
    addUrl={WalterBetriebskostenrechnungEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterBetriebskostenrechnung {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
