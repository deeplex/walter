<script lang="ts">
    import {
        WalterDataWrapper,
        WalterVertragVersion
    } from '$walter/components';
    import { WalterVertragVersionEntry } from '$walter/lib';
    import { walter_goto } from '$walter/services/utils';
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

    const navigate = (e: CustomEvent<DataTableRow>) =>
        walter_goto(`/vertragversionen/${e.detail.id}`);

    export let entry: Partial<WalterVertragVersionEntry> | undefined =
        undefined;
</script>

<WalterDataWrapper
    addUrl={WalterVertragVersionEntry.ApiURL}
    {navigate}
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
