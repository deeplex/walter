<script lang="ts">
    import { WalterAnhaengeEntry, WalterHeader } from '$walter/components';
    import { Button, Content, InlineLoading, Loading } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import { WalterS3FileWrapper, WalterToastContent } from '$walter/lib';
    import { walter_s3_get_files, walter_s3_remove_folder_content } from '$walter/services/s3';

    export let data: PageData;
    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
    fileWrapper.register("Papierkorb", data.S3URL);

    let trashFiles = fileWrapper.handles[0].files;
    const updateTrash = () => {
        trashFiles = walter_s3_get_files(data.S3URL, fileWrapper.fetchImpl);
        return "";
    };

    async function emptyTrash()
    {     
        const files = await trashFiles;
        const toast = new WalterToastContent(
            'Alle Dateien endgültig entfernt',
            'Löschen aller Dateien fehlgeschlagen.',
            updateTrash, updateTrash);

        await walter_s3_remove_folder_content(files, toast)
    }
</script>

<WalterHeader title="Papierkorb" />

<Content>
    {#await trashFiles}
        <Loading />
    {:then files}
        <Button
            disabled={files.length === 0}
            on:click={emptyTrash}>Papierkorb leeren.</Button>
        {#each files as file}
            <WalterAnhaengeEntry {file} bind:fileWrapper />
        {/each}
    {/await}
</Content> 