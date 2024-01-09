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
    import {
        HeaderPanelDivider,
        InlineLoading
    } from 'carbon-components-svelte';
    import { WalterAnhaengeEntry } from '..';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import type { WalterFileHandle } from '$walter/lib';

    export let permissions: WalterPermissions | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let allHandles: WalterFileHandle[];
    export let handle: WalterFileHandle;
    export let filter: string;
</script>

{#await handle.files}
    <HeaderPanelDivider>{handle.name} (-)</HeaderPanelDivider>
    <InlineLoading style="margin-left: 2em" description="Lade Dateien" />
{:then files}
    <HeaderPanelDivider>{handle.name} ({files?.length || 0})</HeaderPanelDivider
    >
    {#each files || [] as file}
        {#if file.fileName.toLowerCase().includes(filter.toLowerCase())}
            <WalterAnhaengeEntry
                bind:allHandles
                {permissions}
                {file}
                {fetchImpl}
                bind:handle
            />
        {/if}
    {/each}
{/await}
