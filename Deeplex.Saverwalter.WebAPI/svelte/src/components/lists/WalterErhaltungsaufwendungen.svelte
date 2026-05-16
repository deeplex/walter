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

    import {
        WalterDataTable,
        WalterErhaltungsaufwendung
    } from '$walter/components';
    import { WalterErhaltungsaufwendungEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let rows: WalterErhaltungsaufwendungEntry[] | undefined = undefined;
    export let entry: Partial<WalterErhaltungsaufwendungEntry> | undefined = {};

    const headers = [
        { key: 'bezeichnung', value: 'Bezeichnung' },
        { key: 'aussteller.text', value: 'Aussteller' },
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'betrag', value: 'Betrag' },
        { key: 'datum', value: 'Datum' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.erhaltungsaufwendung(e.detail.id);
    const rowHref = (row: DataTableRow) => `/erhaltungsaufwendungen/${row.id}`;

    const fetchData = rows === undefined
        ? (p: Parameters<typeof WalterErhaltungsaufwendungEntry.GetPaged>[1]) =>
              WalterErhaltungsaufwendungEntry.GetPaged<WalterErhaltungsaufwendungEntry>(fetchImpl, p)
        : undefined;
</script>

<WalterDataTable
    addUrl={WalterErhaltungsaufwendungEntry.ApiURL}
    addEntry={entry}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    {on_click_row}
    {rowHref}
    {rows}
    {fetchData}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterErhaltungsaufwendung {fetchImpl} {entry} />
    {/if}
</WalterDataTable>
