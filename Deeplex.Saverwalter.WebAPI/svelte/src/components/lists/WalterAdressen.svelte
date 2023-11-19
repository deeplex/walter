<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterAdresse, WalterDataWrapper } from '$walter/components';
    import { WalterAdresseEntry } from '$walter/lib';
    import { walter_goto } from '$walter/services/utils';

    const headers = [
        { key: 'strasse', value: 'Straße' },
        { key: 'hausnummer', value: 'Hausnummer' },
        { key: 'postleitzahl', value: 'Postleitzahl' },
        { key: 'stadt', value: 'Stadt' }
    ];

    const navigate = (e: CustomEvent<DataTableRow>) =>
        walter_goto(`/adressen/${e.detail.id}`);

    export let fullHeight = false;
    export let rows: WalterAdresseEntry[];
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterAdresseEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterAdresseEntry.ApiURL}
    addEntry={entry}
    {title}
    {navigate}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterAdresse {entry} />
    {/if}
</WalterDataWrapper>
