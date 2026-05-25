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

    import { WalterDataTable } from '$walter/components';
    import { WalterBankkontoEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import WalterBankkonto from '../details/WalterBankkonto.svelte';

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let rows: WalterBankkontoEntry[] | undefined = undefined;
    export let entry: Partial<WalterBankkontoEntry> = {
        permissions: { read: true, update: true, remove: true }
    };

    const headers = [
        { key: 'bezeichnung', value: 'Bezeichnung', default: '' },
        { key: 'iban', value: 'IBAN' },
        { key: 'bank', value: 'Bank' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.bankkonto(e.detail.id);
    const rowHref = (row: DataTableRow) => `/bankkontos/${row.id}`;

    const fetchData =
        rows === undefined
            ? (p: Parameters<typeof WalterBankkontoEntry.GetPaged>[1]) =>
                  WalterBankkontoEntry.GetPaged<WalterBankkontoEntry>(
                      fetchImpl,
                      p
                  )
            : undefined;
</script>

<WalterDataTable
    addUrl={WalterBankkontoEntry.ApiURL}
    addEntry={entry}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    {on_click_row}
    {rowHref}
    {rows}
    {fetchData}
    initialSortBy="bezeichnung"
    initialSortDir="asc"
    {headers}
    {fullHeight}
>
    <WalterBankkonto {fetchImpl} {entry} />
</WalterDataTable>
