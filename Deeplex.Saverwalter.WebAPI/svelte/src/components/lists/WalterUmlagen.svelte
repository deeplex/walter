<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterUmlage } from '$walter/components';
    import type { WalterSelectionEntry, WalterUmlageEntry } from '$walter/lib';

    const headers = [
        { key: 'typ.text', value: 'Typ' },
        { key: 'wohnungenBezeichnung', value: 'Wohnungen' }
    ];

    const addUrl = `/api/umlagen/`;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/umlagen/${e.detail.id}`);

    export let fullHeight = false;
    export let rows: WalterUmlageEntry[];
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterUmlageEntry> | undefined = undefined;
    export let fetchImpl: typeof fetch;
</script>

<WalterDataWrapper
    addEntry={entry}
    {addUrl}
    {title}
    {navigate}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterUmlage {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
