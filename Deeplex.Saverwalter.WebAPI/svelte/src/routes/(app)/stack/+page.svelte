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

    async function upload() {
        for (const file of newFiles) {
            {
                walter_s3_post(file, data.S3URL, data.fetch).then(() =>
                    upload_finished(file)
                );
            }
        }
    }
</script>

<WalterHeader title="Ablagestapel">
    <HeaderUtilities>
        <HeaderAction text="({data.files.length})">
            <WalterAnhaenge
                f={data.fetch}
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
