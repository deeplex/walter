<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterVertrag } from '$walter/components';
    import type { WalterSelectionEntry, WalterVertragEntry } from '$walter/lib';

    const headers = [
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'mieterAuflistung', value: 'Mieter' },
        { key: 'beginn', value: 'Beginn' },
        { key: 'ende', value: 'Ende' }
    ];

    const addUrl = `/api/vertraege/`;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/vertraege/${e.detail.id}`);

    export let rows: WalterVertragEntry[];
    export let search = false;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let entry: Partial<WalterVertragEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    {addUrl}
    addEntry={entry}
    {title}
    {search}
    {navigate}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterVertrag {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
