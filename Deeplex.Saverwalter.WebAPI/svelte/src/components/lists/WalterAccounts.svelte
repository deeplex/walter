<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterAccount, WalterDataWrapper } from '$walter/components';
    import { WalterAccountEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'username', value: 'Nutzername' },
        { key: 'name', value: 'Anzeigename' },
        { key: 'passwordlink', value: 'Passwortlink' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.account(e.detail.id);

    export let fullHeight = false;
    export let rows: WalterAccountEntry[];
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterAccountEntry> | undefined = undefined;
    export let fetchImpl: typeof fetch;
</script>

<WalterDataWrapper
    addUrl={WalterAccountEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterAccount {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
