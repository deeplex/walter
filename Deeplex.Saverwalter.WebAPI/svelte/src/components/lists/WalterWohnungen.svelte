<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterWohnung } from '$walter/components';
    import { WalterWohnungEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'adresse.anschrift', value: 'Anschrift' },
        { key: 'bezeichnung', value: 'Bezeichnung' },
        { key: 'besitzer.text', value: 'Besitzer' },
        { key: 'bewohner', value: 'Bewohner' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.wohnung(e.detail.id);

    export let rows: WalterWohnungEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let entry: Partial<WalterWohnungEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterWohnungEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterWohnung {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
