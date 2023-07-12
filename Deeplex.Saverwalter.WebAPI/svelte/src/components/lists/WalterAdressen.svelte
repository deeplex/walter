<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterAdresse, WalterDataWrapper } from '$walter/components';
    import type { WalterAdresseEntry } from '$walter/lib';

    const headers = [
        { key: 'strasse', value: 'Straße' },
        { key: 'hausnummer', value: 'Hausnummer' },
        { key: 'postleitzahl', value: 'Postleitzahl' },
        { key: 'stadt', value: 'Stadt' }
    ];

    const addUrl = `/api/wohnungen/`;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/adressen/${e.detail.id}`);

    export let fullHeight = false;
    export let rows: WalterAdresseEntry[];
    export let search = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterAdresseEntry> | undefined = undefined;
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
        <WalterAdresse {entry} />
    {/if}
</WalterDataWrapper>
