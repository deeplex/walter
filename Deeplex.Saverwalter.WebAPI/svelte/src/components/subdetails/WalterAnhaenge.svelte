<script lang="ts">
    import {
        FileUploaderDropContainer,
        HeaderPanelDivider,
        HeaderPanelLink,
        HeaderPanelLinks,
        InlineLoading
    } from 'carbon-components-svelte';

    import {
        create_walter_s3_file_from_file,
        walter_s3_get_files,
        walter_s3_post
    } from '$walter/services/s3';
    import type { WalterS3File } from '$walter/types';
    import { openModal } from '$walter/store';
    import WalterAnhaengeEntry from './WalterAnhaengeEntry.svelte';
    import { onMount } from 'svelte';
    import type { WalterS3FileWrapper } from '$walter/lib';

    export let fileWrapper: WalterS3FileWrapper;

    let fileUploadComplete = false;
    let newFiles: File[] = [];

    // TODO replace:
    // function upload_finished(file: File) {
    //     fileUploadComplete = true;
    //     // Don't update if file already exists (file overwrite)
    //     if (files.some((e) => e.FileName == file.name)) {
    //         return;
    //     }
    //     files = [...files, create_walter_s3_file_from_file(file, S3URL)];
    // }

    // TODO replace:
    // function post_s3_file(file: File) {
    //     walter_s3_post(file, S3URL, fetchImpl).then(() =>
    //         upload_finished(file)
    //     );
    // }

    // TODO replace:
    // async function upload() {
    //     for (const file of newFiles) {
    //         {
    //             if (files.map((e) => e.FileName).includes(file.name)) {
    //                 const content = `Eine Datei mit dem Namen ${file.name} existiert bereits in dieser Ablage. Bist du sicher, dass diese Datei hochgeladen werden soll?`;
    //                 openModal({
    //                     modalHeading: `Datei existiert bereits`,
    //                     content,
    //                     primaryButtonText: 'Überschreiben',
    //                     submit: () => post_s3_file(file)
    //                 });
    //             } else {
    //                 post_s3_file(file);
    //             }
    //         }
    //     }
    // }
</script>

<HeaderPanelLinks>
    <FileUploaderDropContainer
        style="scale: 0.9; margin-top: -2em"
        bind:files={newFiles}
        on:add={() => /* upload */ undefined}
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
                <WalterAnhaengeEntry {file} {fileWrapper} />
            {/each}
        {/await}
    {/each}
</HeaderPanelLinks>
