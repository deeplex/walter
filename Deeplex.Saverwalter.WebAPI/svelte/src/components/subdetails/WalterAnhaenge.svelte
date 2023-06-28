<script lang="ts">
    import {
        FileUploaderDropContainer,
        HeaderPanelDivider,
        HeaderPanelLinks,
        InlineLoading
    } from 'carbon-components-svelte';

    import {
        create_walter_s3_file_from_file,
        walter_s3_post
    } from '$walter/services/s3';
    import { openModal } from '$walter/store';
    import WalterAnhaengeEntry from './WalterAnhaengeEntry.svelte';
    import type { WalterS3FileWrapper } from '$walter/lib';

    export let fileWrapper: WalterS3FileWrapper;

    let fileUploadComplete = false;
    let newFiles: File[] = [];

    function upload_finished(file: File) {
        fileUploadComplete = true;
        fileWrapper.addFile(
            create_walter_s3_file_from_file(file, fileWrapper.handles[0].S3URL),
            0
        );
    }

    function post_s3_file(file: File) {
        walter_s3_post(
            file,
            fileWrapper.handles[0].S3URL,
            fileWrapper.fetchImpl
        ).then(() => upload_finished(file));
    }

    async function upload() {
        const files = await fileWrapper.handles[0].files;
        for (const file of newFiles) {
            {
                if (files.map((e) => e.FileName).includes(file.name)) {
                    const content = `Eine Datei mit dem Namen ${file.name} existiert bereits in dieser Ablage. Bist du sicher, dass diese Datei hochgeladen werden soll?`;
                    openModal({
                        modalHeading: `Datei existiert bereits`,
                        content,
                        primaryButtonText: 'Überschreiben',
                        submit: () => post_s3_file(file)
                    });
                } else {
                    post_s3_file(file);
                }
            }
        }
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
