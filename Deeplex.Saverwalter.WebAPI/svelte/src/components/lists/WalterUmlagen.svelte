<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterUmlage } from '$walter/components';
    import { WalterUmlageEntry } from '$walter/lib';
    import { walter_goto } from '$walter/services/utils';

    const headers = [
        { key: 'typ.text', value: 'Typ' },
        { key: 'wohnungenBezeichnung', value: 'Wohnungen' }
    ];

    const navigate = (e: CustomEvent<DataTableRow>) =>
        walter_goto(`/umlagen/${e.detail.id}`);

    export let fullHeight = false;
    export let rows: WalterUmlageEntry[];
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterUmlageEntry> | undefined = undefined;
    export let fetchImpl: typeof fetch;
</script>

<WalterDataWrapper
    addUrl={WalterUmlageEntry.ApiURL}
    addEntry={entry}
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
