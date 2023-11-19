<script lang="ts">
    import {
        WalterDataWrapper,
        WalterVertragVersion
    } from '$walter/components';
    import { WalterVertragVersionEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    const headers = [
        { key: 'beginn', value: 'Beginn' },
        { key: 'personenzahl', value: 'Personenzahl' },
        { key: 'grundmiete', value: 'Grundmiete' },
        { key: 'notiz', value: 'Notiz' }
    ];

    export let rows: WalterVertragVersionEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.vertragversion(e.detail.id);

    export let entry: Partial<WalterVertragVersionEntry> | undefined =
        undefined;
</script>

<WalterDataWrapper
    addUrl={WalterVertragVersionEntry.ApiURL}
    {on_click_row}
    addEntry={entry}
    {title}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterVertragVersion {entry} />
    {/if}
</WalterDataWrapper>
