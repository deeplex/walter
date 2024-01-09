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
    import { AccordionItem, Tile } from 'carbon-components-svelte';

    import { WalterDataTable } from '$walter/components';
    import { page } from '$app/stores';
    import WalterDataWrapperQuickAdd from './WalterDataWrapperQuickAdd.svelte';
    import { walter_goto } from '$walter/services/utils';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';

    export let readonly = false;
    export let fullHeight = false;
    export let addUrl: string | undefined = undefined;
    export let addEntry: { permissions?: WalterPermissions } | undefined =
        undefined;
    export let title: string | undefined = undefined;
    export let rows: unknown[];
    export let headers: {
        key: string;
        value: string;
    }[];
    export let on_click_row: (
        e: CustomEvent
    ) => Promise<void> | void = () => {};

    let open = false;
    let addModalOpen = false;
    let quick_add = () => {
        addModalOpen = true;
    };

    function normal_add() {
        walter_goto(`${$page.url.pathname}/new`);
    }

    function onSubmit(new_value: unknown) {
        rows = [...rows, new_value];
    }
</script>

{#if title !== undefined}
    {#if addUrl}
        <WalterDataWrapperQuickAdd
            bind:addEntry
            {addUrl}
            bind:addModalOpen
            {onSubmit}
            {title}
        >
            <slot />
        </WalterDataWrapperQuickAdd>
    {/if}

    <AccordionItem title={`${title} (${rows.length})`} bind:open>
        <Tile style="overflow: auto">
            <WalterDataTable
                add={addUrl && addEntry?.permissions?.update
                    ? quick_add
                    : undefined}
                {on_click_row}
                bind:rows
                {headers}
            />
        </Tile>
    </AccordionItem>
{:else}
    <WalterDataTable
        {readonly}
        add={normal_add}
        {fullHeight}
        {on_click_row}
        {rows}
        {headers}
    />
{/if}
