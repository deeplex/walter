<!-- Copyright (C) 2023-2025  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterTransaktion } from '$walter/components';
    import { WalterTransaktionEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    export let fetchImpl: typeof fetch;

    const headers = [
        { key: 'zahler.text', value: 'Zahler' },
        { key: 'betrag', value: 'Betrag' },
        {
            key: 'zahlungsempfaenger.text',
            value: 'Zahlungsempfänger'
        },
        { key: 'zahlungsdatum', value: 'Zahlungsdatum' },
        { key: 'verwendungszweck', value: 'Memo' }
    ];

    export let rows: WalterTransaktionEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterTransaktionEntry> | undefined = undefined;

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.transaktion(e.detail.id);
</script>

<WalterDataWrapper
    addUrl={WalterTransaktionEntry.ApiURL}
    {on_click_row}
    addEntry={entry}
    {title}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterTransaktion {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
