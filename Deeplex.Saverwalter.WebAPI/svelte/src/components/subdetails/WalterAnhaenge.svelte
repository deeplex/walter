<script lang="ts">
    import {
        FileUploaderDropContainer,
        HeaderPanelDivider,
        HeaderPanelLinks,
        InlineLoading
    } from 'carbon-components-svelte';

    import WalterAnhaengeEntry from './WalterAnhaengeEntry.svelte';
    import type { WalterS3FileWrapper } from '$walter/lib';
    import { upload_new_files } from './WalterAnhaenge';

    export let fileWrapper: WalterS3FileWrapper;

    let newFiles: File[] = [];

    function upload(e: CustomEvent<readonly File[]>): void {
        upload_new_files(fileWrapper, newFiles);
    }
</script>

<HeaderPanelLinks>
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
                <WalterAnhaengeEntry {file} bind:fileWrapper />
            {/each}
        {/await}
    {/each}
</HeaderPanelLinks>
