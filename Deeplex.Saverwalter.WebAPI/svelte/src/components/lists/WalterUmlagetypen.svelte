<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterUmlagetyp } from '$walter/components';
    import type { WalterUmlagetypEntry } from '$walter/lib';
    import { walter_goto } from '$walter/services/utils';

    const headers = [{ key: 'bezeichnung', value: 'Bezeichnung' }];

    const addUrl = `/api/umlagetypen/`;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        walter_goto(`/umlagetypen/${e.detail.id}`);

    export let fullHeight = false;
    export let rows: WalterUmlagetypEntry[];
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterUmlagetypEntry> | undefined = undefined;
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
        <WalterUmlagetyp {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
