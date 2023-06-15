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

    export let rows: WalterUmlageEntry[];
    export let search = false;
    export let title: string | undefined = undefined;
    export let betriebskostentypen: WalterSelectionEntry[];
    export let umlageschluessel: WalterSelectionEntry[];
    export let wohnungen: WalterSelectionEntry[];
    export let zaehler: WalterSelectionEntry[];
    export let entry: Partial<WalterUmlageEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addEntry={entry}
    {addUrl}
    {title}
    {search}
    {navigate}
    {rows}
    {headers}
>
    {#if entry}
        <WalterUmlage
            {zaehler}
            {betriebskostentypen}
            {umlageschluessel}
            {wohnungen}
            {entry}
        />
    {/if}
</WalterDataWrapper>
