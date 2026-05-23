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
    import { WalterDataTable, WalterGarageVertrag, WalterGarageVertragVersion } from '$walter/components';
    import {
        WalterGarageVertragEntry,
        type WalterGarageVertragVersionEntry,
        validateGarageVertragQuickAdd
    } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let entry: Partial<WalterGarageVertragEntry> | undefined = {};
    export let rows: WalterGarageVertragEntry[] | undefined = undefined;

    let entryVersion: Partial<WalterGarageVertragVersionEntry> = {};
    let entryVersionBeginn: string | undefined = undefined;
    let hasOverlap = false;
    $: if (entry) entry.versionen = [entryVersion as WalterGarageVertragVersionEntry];
    $: submitDisabled = !validateGarageVertragQuickAdd(entry) || hasOverlap;

    const headers = [
        { key: 'garage.text', value: 'Garage' },
        { key: 'mieterAuflistung', value: 'Mieter' },
        { key: 'aktuelleGaragenMiete', value: 'Miete (€)' },
        { key: 'beginn', value: 'Beginn' },
        { key: 'ende', value: 'Ende' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.garageVertrag(e.detail.id);
    const rowHref = (row: DataTableRow) => `/garage-vertraege/${row.id}`;

    const fetchData =
        rows === undefined
            ? (p: Parameters<typeof WalterGarageVertragEntry.GetPaged>[1]) =>
                  WalterGarageVertragEntry.GetPaged<WalterGarageVertragEntry>(fetchImpl, p)
            : undefined;
</script>

<WalterDataTable
    addUrl={WalterGarageVertragEntry.ApiURL}
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
        <WalterGarageVertrag {fetchImpl} bind:entry />
        <WalterGarageVertragVersion
            bind:hasOverlap
            {fetchImpl}
            bind:entry={entryVersion}
            bind:garageVertrag={entry}
            bind:beginn={entryVersionBeginn}
        />
    {/if}
</WalterDataTable>
