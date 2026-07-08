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

    import { WalterDataTable, WalterVertrag } from '$walter/components';
    import { WalterVertragEntry } from '$walter/lib';
    import { validateVertragQuickAdd } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let entry: Partial<WalterVertragEntry> | undefined = undefined;
    export let rows: WalterVertragEntry[] | undefined = undefined;

    let hasOverlap = false;
    $: submitDisabled = !validateVertragQuickAdd(entry) || hasOverlap;

    const headers = [
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'mieterAuflistung', value: 'Mieter' },
        { key: 'beginn', value: 'Beginn' },
        { key: 'ende', value: 'Ende' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.vertrag(e.detail.id);
    const rowHref = (row: DataTableRow) => `/vertraege/${row.id}`;

    const fetchData =
        rows === undefined
            ? (p: Parameters<typeof WalterVertragEntry.GetPaged>[1]) =>
                  WalterVertragEntry.GetPaged<WalterVertragEntry>(fetchImpl, p)
            : undefined;
</script>

<WalterDataTable
    addUrl={entry !== undefined ? WalterVertragEntry.ApiURL : undefined}
    addEntry={entry}
    {submitDisabled}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    {on_click_row}
    {rowHref}
    {rows}
    {fetchData}
    initialSortBy="beginn"
    initialSortDir="desc"
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterVertrag {fetchImpl} bind:entry bind:hasOverlap />
    {/if}
</WalterDataTable>
