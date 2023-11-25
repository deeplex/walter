<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterDataWrapper,
        WalterTextInput
    } from '$walter/components';
    import { WalterKontaktEntry
    } from '$walter/lib';
    import { ContentSwitcher, Row, Switch } from 'carbon-components-svelte';
    import { navigation } from '$walter/services/navigation';
    import WalterKontakt from '../details/WalterKontakt.svelte';

    const headers = [
        { key: 'bezeichnung', value: 'Name', default: '' },
        { key: 'adresse.anschrift', value: 'Anschrift' },
        { key: 'telefon', value: 'Telefon' },
        { key: 'mobil', value: 'Mobil' },
        { key: 'email', value: 'E-Mail' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) => navigation.kontakt(e.detail.id);

    export let rows: WalterKontaktEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;

    export let entry: Partial<WalterKontaktEntry> = {};
</script>

<WalterDataWrapper
    addUrl={WalterKontaktEntry.ApiURL}
    addEntry={entry}
    {title}
    {on_click_row}
    {rows}
    {headers}
    {fullHeight}
>
    <WalterKontakt {fetchImpl} entry={entry} />
</WalterDataWrapper>
