<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterWohnung } from '$walter/components';
    import type { WalterSelectionEntry, WalterWohnungEntry } from '$walter/lib';

    const headers = [
        { key: 'adresse.anschrift', value: 'Anschrift' },
        { key: 'bezeichnung', value: 'Bezeichnung' },
        { key: 'besitzer.text', value: 'Besitzer' },
        { key: 'bewohner', value: 'Bewohner' }
    ];

    const addUrl = `/api/wohnungen/`;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/wohnungen/${e.detail.id}`);

    export let rows: WalterWohnungEntry[];
    export let search = false;
    export let title: string | undefined = undefined;
    export let kontakte: WalterSelectionEntry[];
    export let entry: Partial<WalterWohnungEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    {addUrl}
    addEntry={entry}
    {title}
    {search}
    {navigate}
    {rows}
    {headers}
>
    {#if entry}
        <WalterWohnung {kontakte} {entry} />
    {/if}
</WalterDataWrapper>
