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
    import type { ComponentType } from 'svelte';
    import { WalterDataTable } from '$walter/components';

    export let entityClass: { ApiURL: string; GetPaged: Function };
    export let validate: (e: unknown) => boolean = () => true;
    export let headers: { key: string; value: string }[];
    export let navFn: (id: number) => void;
    export let routeBase: string;
    export let formComponent: ComponentType;
    export let initialSortBy: string | undefined = undefined;
    export let initialSortDir: 'asc' | 'desc' = 'asc';
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch | undefined = undefined;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    export let entry: any = {};
    export let rows: unknown[] | undefined = undefined;
    export let readonly = false;

    const on_click_row = (e: CustomEvent<DataTableRow>) => navFn(e.detail.id);
    const rowHref = (row: DataTableRow) => `/${routeBase}/${row.id}`;

    $: fetchData =
        rows === undefined && fetchImpl !== undefined
            ? (p: Parameters<typeof entityClass.GetPaged>[1]) =>
                  entityClass.GetPaged(fetchImpl, p)
            : undefined;

    $: submitDisabled = !validate(entry);
</script>

<WalterDataTable
    {readonly}
    addUrl={entityClass.ApiURL}
    addEntry={entry}
    {submitDisabled}
    layout={title !== undefined ? 'accordion' : 'inline'}
    accordionTitle={title}
    quickAddTitle={title}
    {on_click_row}
    {rowHref}
    {rows}
    {fetchData}
    {initialSortBy}
    {initialSortDir}
    {headers}
    {fullHeight}
>
    <svelte:component this={formComponent} bind:entry {fetchImpl} />
</WalterDataTable>
