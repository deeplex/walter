<!-- Copyright (C) 2023-2026  Kai Lawrence -->
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

    import { WalterDataTable, WalterTransaktion } from '$walter/components';
    import { WalterTransaktionEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    export let fetchImpl: typeof fetch;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterTransaktionEntry> | undefined = {};
    export let rows: WalterTransaktionEntry[] | undefined = undefined;

    let transaktionIsValid = false;

    const headers = [
        { key: 'zahler.text', value: 'Zahler' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'zahlungsempfaenger.text', value: 'Zahlungsempfänger' },
        { key: 'zahlungsdatum', value: 'Zahlungsdatum' },
        { key: 'verwendungszweck', value: 'Memo' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.transaktion(e.detail.id);
    const rowHref = (row: DataTableRow) => `/transaktionen/${row.id}`;

    const fetchData =
        rows === undefined
            ? (p: Parameters<typeof WalterTransaktionEntry.GetPaged>[1]) =>
                  WalterTransaktionEntry.GetPaged<WalterTransaktionEntry>(
                      fetchImpl,
                      p
                  )
            : undefined;
</script>

<WalterDataTable
    addUrl={WalterTransaktionEntry.ApiURL}
    {on_click_row}
    {rowHref}
    addEntry={entry}
    submitDisabled={!transaktionIsValid}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    {rows}
    {fetchData}
    initialSortBy="zahlungsdatum"
    initialSortDir="desc"
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterTransaktion {fetchImpl} bind:isValid={transaktionIsValid} />
    {/if}
</WalterDataTable>
