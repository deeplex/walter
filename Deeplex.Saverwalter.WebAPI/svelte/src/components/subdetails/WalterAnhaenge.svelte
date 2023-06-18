<script lang="ts">
    import {
        FileUploaderDropContainer,
        HeaderPanelDivider,
        HeaderPanelLinks
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

    export let fetchImpl: typeof fetch;
    export let S3URL: string;
    export let files: WalterS3File[];
    export let refFiles: WalterS3File[] | undefined = undefined;
    export let hideStackFiles = false;

    let stackFiles: WalterS3File[] | undefined = undefined;

    if (!hideStackFiles) {
        onMount(async () => {
            stackFiles = await walter_s3_get_files('stack', fetchImpl);
        });
    }

    let fileUploadComplete = false;
    let newFiles: File[] = [];

    function upload_finished(file: File) {
        fileUploadComplete = true;
        // Don't update if file already exists (file overwrite)
        if (files.some((e) => e.FileName == file.name)) {
            return;
        }
        files = [...files, create_walter_s3_file_from_file(file, S3URL)];
    }

    function post_s3_file(file: File) {
        walter_s3_post(file, S3URL, fetchImpl).then(() =>
            upload_finished(file)
        );
    }

    async function upload() {
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
    <HeaderPanelDivider>Dateien ({files.length})</HeaderPanelDivider>
    <HeaderPanelLinks>
        {#each files as file}
            <WalterAnhaengeEntry {file} bind:files {fetchImpl} />
        {/each}
        {#if refFiles}
            <HeaderPanelDivider
                >Bezugsdateien ({refFiles.length})</HeaderPanelDivider
            >
            {#each refFiles as refFile}
                <WalterAnhaengeEntry
                    file={refFile}
                    bind:files={refFiles}
                    {fetchImpl}
                />
            {/each}
        {/if}
        {#if !hideStackFiles && stackFiles}
            <HeaderPanelDivider
                >Ablagestapel ({stackFiles.length})</HeaderPanelDivider
            >
            {#each stackFiles as stackFile}
                <WalterAnhaengeEntry
                    file={stackFile}
                    bind:files={stackFiles}
                    {fetchImpl}
                />
            {/each}
        {/if}
    </HeaderPanelLinks>
</HeaderPanelLinks>
