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
    import type { WalterS3File } from '$walter/types';

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
        walter_s3_post(file, data.S3URL, data.fetch).then(() =>
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
</script>

<WalterHeader title="Ablagestapel">
    <HeaderUtilities>
        <HeaderAction text="({data.files.length})">
            <WalterAnhaenge
                fetchImpl={data.fetch}
                files={data.files}
                S3URL={data.S3URL}
            />
        </HeaderAction>
    </HeaderUtilities>
</WalterHeader>

<Content>
    <FileUploaderDropContainer
        multiple
        labelText="Hier klicken oder Dateien ablegen um sie hochzuladen."
        bind:files={newFiles}
        on:add={upload}
    />
</Content>
