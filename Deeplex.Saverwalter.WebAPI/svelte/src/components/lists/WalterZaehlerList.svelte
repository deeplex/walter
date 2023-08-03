<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterZaehler } from '$walter/components';
    import type { WalterSelectionEntry, WalterZaehlerEntry } from '$walter/lib';

    const headers = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'typ.text', value: 'Typ' },
        { key: 'lastZaehlerstand.datum', value: 'Letztes Ablesedatum' },
        { key: 'lastZaehlerstand.stand', value: 'Letzter Stand' },
        { key: 'lastZaehlerstand.einheit', value: 'Einheit' }
    ];

    const addUrl = `/api/zaehler/`;

    export let rows: WalterZaehlerEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterZaehlerEntry> | undefined = undefined;
    export let fetchImpl: typeof fetch;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/zaehler/${e.detail.id}`);
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
        <WalterZaehler {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
