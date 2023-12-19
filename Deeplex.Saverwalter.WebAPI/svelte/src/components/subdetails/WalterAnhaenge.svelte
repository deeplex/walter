<script lang="ts">
    import {
        FileUploaderDropContainer,
        HeaderPanelLinks,
        InlineLoading,
        TextInput
    } from 'carbon-components-svelte';

    import type { WalterS3FileWrapper } from '$walter/lib';
    import { upload_new_files } from './WalterAnhaenge';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import WalterAnhaengeHandle from './WalterAnhaengeHandle.svelte';
    import type { WalterS3FileHandle } from '$walter/lib/WalterS3FileWrapper';

    export let fileWrapper: WalterS3FileWrapper;
    export let permissions: WalterPermissions | undefined = undefined;

    let newFiles: File[] = [];
    let handles: WalterS3FileHandle[] = fileWrapper.handles;

    async function upload() {
        await upload_new_files(
            fileWrapper.handles[0],
            newFiles,
            fileWrapper.fetchImpl
        );
        handles = fileWrapper.handles;
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
    {#each handles as handle, i}
        <WalterAnhaengeHandle
            bind:filter
            fetchImpl={fileWrapper.fetchImpl}
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
