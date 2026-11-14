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
        Select,
        SelectItem,
        TextInput
    } from 'carbon-components-svelte';

    import { upload_new_files } from './WalterAnhaenge';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import WalterAnhaengeHandle from './WalterAnhaengeHandle.svelte';
    import { noYear, selectableYears, yearLabel } from '$walter/lib';
    import type { WalterFileWrapper, WalterFileHandle } from '$walter/lib';
    import { fileURL } from '$walter/services/files';

    export let fileWrapper: WalterFileWrapper;
    export let permissions: WalterPermissions | undefined = undefined;

    let newFiles: File[] = [];
    let allHandles: WalterFileHandle[] = fileWrapper.handles;

    // In dieses Jahr kommen die neuen Dateien; der Ablagestapel hat keine Jahre. Voreingestellt
    // ist das Jahr des Objekts (z. B. das Datum eines Zählerstands), sonst „Ohne Jahr“.
    const currentYear = new Date().getFullYear();
    let year = noYear;
    let yearChosen = false;
    $: years = [...new Set([...selectableYears(currentYear), year])]
        .filter((y) => y > 0)
        .sort((a, b) => b - a);
    $: hasYears = fileWrapper.handles[0]?.fileURL !== fileURL.stack;
    $: fileWrapper.handles[0]?.files.then(() => {
        const preset = fileWrapper.handles[0]?.defaultYear;
        if (!yearChosen && preset !== undefined) year = preset;
    });

    async function upload() {
        await upload_new_files(
            fileWrapper.handles[0],
            newFiles,
            fileWrapper.fetchImpl,
            hasYears ? year : undefined
        );
        allHandles = fileWrapper.handles;
        newFiles = [];
    }

    let filter = '';
</script>

<HeaderPanelLinks>
    {#if !permissions || permissions.update}
        {#if hasYears}
            <Select
                size="sm"
                labelText="Jahr der neuen Dateien"
                bind:selected={year}
                on:change={() => (yearChosen = true)}
                style="padding: 0 1rem"
            >
                <SelectItem value={0} text={yearLabel(0)} />
                {#each years as y}
                    <SelectItem value={y} text={yearLabel(y)} />
                {/each}
            </Select>
        {/if}
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
