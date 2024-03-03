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
    import type { WalterFile } from '$walter/types';

    import { get_file_and_update_url } from './WalterAnhaengeEntry';
    import { page } from '$app/stores';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import {
        HeaderPanelLink,
        TooltipDefinition
    } from 'carbon-components-svelte';
    import { WalterPreview } from '..';
    import type { WalterFileHandle } from '$walter/lib';

    export let file: WalterFile;
    export let fetchImpl: typeof fetch;
    export let permissions: WalterPermissions | undefined;
    export let handle: WalterFileHandle;
    export let allHandles: WalterFileHandle[];

    async function showModal() {
        selectedFile = await get_file_and_update_url(file);
        previewOpen = true;
    }

    let selectedFile: WalterFile;
    let previewOpen = false;

    const style = `
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        width: 14em;
        height: 2rem;
        color: #c6c6c6;
        text-align: left;
        padding-top: 7px`;
</script>

{#if selectedFile}
    <WalterPreview
        {permissions}
        {fetchImpl}
        bind:allHandles
        bind:handle
        bind:file={selectedFile}
        bind:open={previewOpen}
    />
{/if}

<HeaderPanelLink
    style={file.fileName === new URL($page.url).searchParams.get('file')
        ? 'background-color: #393939;'
        : ''}
    on:click={showModal}
>
    <TooltipDefinition style="top: -7px;" align="start" direction="top">
        <svelte:fragment slot="tooltip">
            <div style="width: 14em;">
                {file.fileName}
            </div>
        </svelte:fragment>
        <p {style}>
            {file.fileName}
        </p>
    </TooltipDefinition>
</HeaderPanelLink>
