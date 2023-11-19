<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterDataWrapper,
        WalterErhaltungsaufwendung
    } from '$walter/components';
    import { WalterErhaltungsaufwendungEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'bezeichnung', value: 'Bezeichnung' },
        { key: 'aussteller.text', value: 'Aussteller' },
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'datum', value: 'Datum' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.erhaltungsaufwendung(e.detail.id);

    export let fullHeight = false;
    export let rows: WalterErhaltungsaufwendungEntry[];
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;

    export let entry: Partial<WalterErhaltungsaufwendungEntry> | undefined =
        undefined;
</script>

<WalterDataWrapper
    addUrl={WalterErhaltungsaufwendungEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterErhaltungsaufwendung {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
