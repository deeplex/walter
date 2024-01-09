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
        FileUploaderDropContainer,
        HeaderPanelLinks,
        InlineLoading,
        TextInput
    } from 'carbon-components-svelte';

    import { upload_new_files } from './WalterAnhaenge';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import WalterAnhaengeHandle from './WalterAnhaengeHandle.svelte';
    import type { WalterFileWrapper, WalterFileHandle } from '$walter/lib';

    export let fileWrapper: WalterFileWrapper;
    export let permissions: WalterPermissions | undefined = undefined;

    let newFiles: File[] = [];
    let allHandles: WalterFileHandle[] = fileWrapper.handles;

    async function upload() {
        await upload_new_files(
            fileWrapper.handles[0],
            newFiles,
            fileWrapper.fetchImpl
        );
        allHandles = fileWrapper.handles;
        newFiles = [];
    }

    let filter = '';
</script>

<HeaderPanelLinks>
    {#if !permissions || permissions.update}
        <FileUploaderDropContainer
            style="scale: 0.9; margin-top: -2em"
            bind:files={newFiles}
            on:add={upload}
            multiple
        >
            <svelte:fragment slot="labelText"
                ><div style="font-size: medium">
                    Hier klicken oder Dateien ablegen um sie hochzuladen.
                </div></svelte:fragment
            >
        </FileUploaderDropContainer>
    {/if}

    <TextInput
        bind:value={filter}
        placeholder="Suche..."
        size="sm"
        style="background-color: transparent; color: white"
    ></TextInput>
    {#each allHandles as handle, i}
        <WalterAnhaengeHandle
            bind:filter
            fetchImpl={fileWrapper.fetchImpl}
            bind:allHandles
            {permissions}
            bind:handle
        />
        {#if i === 0}
            {#each newFiles as file}
                <InlineLoading
                    style="margin-left: 1em"
                    description={file.name}
                />
            {/each}
        {/if}
    {/each}
</HeaderPanelLinks>
