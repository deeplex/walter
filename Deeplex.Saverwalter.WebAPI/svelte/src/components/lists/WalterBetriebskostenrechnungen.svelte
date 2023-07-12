<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterBetriebskostenrechnung,
        WalterDataWrapper
    } from '$walter/components';
    import type {
        WalterBetriebskostenrechnungEntry,
        WalterSelectionEntry
    } from '$walter/lib';

    export let fullHeight = false;
    export let rows: WalterBetriebskostenrechnungEntry[];
    export let search = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;

    const headers = [
        { key: 'typ.text', value: 'Typ' },
        { key: 'umlage.text', value: 'Wohnungen' },
        { key: 'betreffendesJahr', value: 'Betreffendes Jahr' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'datum', value: 'Datum' }
    ];

    const addUrl = `/api/betriebskostenrechnungen/`;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/betriebskostenrechnungen/${e.detail.id}`);

    export let entry: Partial<WalterBetriebskostenrechnungEntry> | undefined =
        undefined;
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
        <WalterBetriebskostenrechnung {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
