<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterUmlagetyp } from '$walter/components';
    import { WalterUmlagetypEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [{ key: 'bezeichnung', value: 'Bezeichnung' }];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.umlagetyp(e.detail.id);

    export let fullHeight = false;
    export let rows: WalterUmlagetypEntry[];
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterUmlagetypEntry> | undefined = undefined;
    export let fetchImpl: typeof fetch;
</script>

<WalterDataWrapper
    addUrl={WalterUmlagetypEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterUmlagetyp {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
