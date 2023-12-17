<script lang="ts">
    import {
        FileUploaderDropContainer,
        HeaderPanelDivider,
        HeaderPanelLinks,
        InlineLoading,
        TextInput
    } from 'carbon-components-svelte';

    import WalterAnhaengeEntry from './WalterAnhaengeEntry.svelte';
    import type { WalterS3FileWrapper } from '$walter/lib';
    import { upload_new_files } from './WalterAnhaenge';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';

    export let fileWrapper: WalterS3FileWrapper;
    export let permissions: WalterPermissions | undefined;

    let newFiles: File[] = [];

    function upload(): void {
        upload_new_files(fileWrapper, newFiles);
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
        {:then loadedFiles}
            <HeaderPanelDivider
                >{handle.name} ({loadedFiles.length})</HeaderPanelDivider
            >
            {#each loadedFiles as file}
                {#if file.FileName.toLowerCase().includes(filter.toLowerCase())}
                    <WalterAnhaengeEntry
                        {permissions}
                        {file}
                        bind:fileWrapper
                    />
                {/if}
            {/each}
        {/await}
    {/each}
</HeaderPanelLinks>
