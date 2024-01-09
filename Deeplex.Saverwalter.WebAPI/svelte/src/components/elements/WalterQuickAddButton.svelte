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
    import { Button } from 'carbon-components-svelte';
    import { Add } from 'carbon-icons-svelte';
    import WalterDataWrapperQuickAdd from './WalterDataWrapperQuickAdd.svelte';

    export let addUrl: string;
    export let addEntry: unknown = {};
    export let onSubmit: undefined | ((e: unknown) => void) = undefined;
    export let title: string | undefined = undefined;

    let addModalOpen = false;
</script>

<div style="margin-top: 34px; margin-left: -0.75em">
    <Button
        on:click={() => {
            addModalOpen = true;
        }}
        icon={Add}
        iconDescription="Hinzufügen"
        size="field"
        kind="tertiary"
    />
</div>
<WalterDataWrapperQuickAdd
    {title}
    bind:addModalOpen
    {addUrl}
    bind:addEntry
    {onSubmit}
>
    {#if addModalOpen}
        <slot />
    {/if}
</WalterDataWrapperQuickAdd>
