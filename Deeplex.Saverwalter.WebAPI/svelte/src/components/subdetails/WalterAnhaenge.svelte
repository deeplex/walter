<script lang="ts">
    import {
        FileUploaderDropContainer,
        HeaderPanelDivider,
        HeaderPanelLink,
        HeaderPanelLinks,
        InlineLoading,
        TextInput
    } from 'carbon-components-svelte';

    import WalterAnhaengeEntry from './WalterAnhaengeEntry.svelte';
    import type { WalterS3FileWrapper } from '$walter/lib';
    import { upload_new_files } from './WalterAnhaenge';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import type { WalterS3File } from '$walter/types';

    export let fileWrapper: WalterS3FileWrapper;
    export let permissions: WalterPermissions | undefined = undefined;

    let newFiles: File[] = [];
    let mainFiles: WalterS3File[] = [];
    fileWrapper.handles[0].files.then((files) => (mainFiles = files));

    async function upload() {
        mainFiles =
            (await upload_new_files(fileWrapper, newFiles)) || mainFiles;
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
    {#each fileWrapper.handles as handle}
        {#await handle.files}
            <HeaderPanelDivider>{handle.name} (-)</HeaderPanelDivider>
            <InlineLoading
                style="margin-left: 2em"
                description="Lade Dateien"
            />
        {:then}
            <HeaderPanelDivider
                >{handle.name} ({mainFiles.length})</HeaderPanelDivider
            >
            {#each mainFiles as file}
                {#if file.FileName.toLowerCase().includes(filter.toLowerCase())}
                    <WalterAnhaengeEntry
                        {permissions}
                        {file}
                        bind:fileWrapper
                    />
                {/if}
            {/each}
        {/await}
        {#each newFiles as file}
            <HeaderPanelLink>
                <div style="display: flex; wrap: nowrap">
                    <InlineLoading style="margin-top: -5px" />
                    <span style="marginLeft: 5px">{file.name}</span>
                </div>
            </HeaderPanelLink>
        {/each}
    {/each}
</HeaderPanelLinks>
