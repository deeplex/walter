<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterAdresse, WalterDataWrapper } from '$walter/components';
    import { WalterAdresseEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'strasse', value: 'Straße' },
        { key: 'hausnummer', value: 'Hausnummer' },
        { key: 'postleitzahl', value: 'Postleitzahl' },
        { key: 'stadt', value: 'Stadt' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.adresse(e.detail.id);

    export let fullHeight = false;
    export let rows: WalterAdresseEntry[];
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterAdresseEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterAdresseEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterAdresse {entry} />
    {/if}
</WalterDataWrapper>
