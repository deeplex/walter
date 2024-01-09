<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
    import { DataTableSkeleton, SkeletonText } from 'carbon-components-svelte';
    import { WalterDataTable } from '..';
    import type { WalterSelectionEntry } from '$walter/lib';

    export let step: number;
    export let rows: WalterSelectionEntry[] | undefined;
    export let selectedEntry: WalterSelectionEntry | undefined;
    export let selectedEntry_change: (e: CustomEvent<unknown>) => void;
    export let entry;

    const headers = [{ key: 'text', value: 'Bezeichnung' }];

    function click() {
        entry = undefined;
        selectedEntry = undefined;
        setTimeout(() => (step = 2), 0);
    }
</script>

{#if step === 1}
    {#if rows}
        <WalterDataTable
            add={click}
            fullHeight
            on_click_row={selectedEntry_change}
            {headers}
            {rows}
        />
    {:else}
        <SkeletonText style="margin: 0; height: 48px" />
        <DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
    {/if}
{/if}
