<script lang="ts">
    import { WalterAnhaenge, WalterHeader } from '$walter/components';
    import {
        Content,
        FileUploaderDropContainer,
        HeaderAction,
        HeaderUtilities
    } from 'carbon-components-svelte';
    import type { PageData } from './$types';
    import {
        create_walter_s3_file_from_file,
        walter_s3_post
    } from '$walter/services/s3';
    import { openModal } from '$walter/store';
    import { WalterS3FileWrapper } from '$walter/lib';

    export let data: PageData;

    let newFiles: File[] = [];

    function upload_finished(file: File) {
        if (data.files.some((e) => e.FileName == file.name)) {
            return;
        }
        data.files = [
            ...data.files,
            create_walter_s3_file_from_file(file, data.S3URL)
        ];
    }

    function post_s3_file(file: File) {
        walter_s3_post(file, data.S3URL, data.fetchImpl).then(() =>
            upload_finished(file)
        );
    }

    async function upload() {
        for (const file of newFiles) {
            {
                if (data.files.map((e) => e.FileName).includes(file.name)) {
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

    let fileWrapper = new WalterS3FileWrapper(data.fetchImpl);
</script>

<WalterHeader title="Ablagestapel">
    <HeaderUtilities>
        <HeaderAction preventCloseOnClickOutside text="({data.files.length})">
            <WalterAnhaenge bind:fileWrapper />
        </HeaderAction>
    </HeaderUtilities>
</WalterHeader>

<Content>
    <div class="FileUploaderDropContainerWrapper">
        <FileUploaderDropContainer
            multiple
            labelText="Hier klicken oder Dateien ablegen um sie hochzuladen."
            bind:files={newFiles}
            on:add={upload}
        />
    </div>
</Content>

<style>
    :global(
            .FileUploaderDropContainerWrapper .bx--file-browse-btn,
            .FileUploaderDropContainerWrapper .bx--file__drop-container
        ) {
        font-size: xx-large;
        flex: 0 0 97%;
        height: 85vh !important;
        max-width: none !important;
        align-items: center;
    }

    :global(.FileUploaderDropContainerWrapper .bx--file__drop-container) {
        padding-left: 20% !important;
    }
</style>
