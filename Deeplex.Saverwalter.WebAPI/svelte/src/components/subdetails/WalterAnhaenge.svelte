<script lang="ts">
    import {
        FileUploader,
        HeaderPanelDivider,
        HeaderPanelLink,
        HeaderPanelLinks,
        Truncate
    } from 'carbon-components-svelte';

    import {
        create_walter_s3_file_from_file,
        walter_s3_get,
        walter_s3_post
    } from '$walter/services/s3';
    import { WalterPreview } from '$walter/components';
    import type { WalterS3File } from '$walter/types';

    export let f: typeof fetch;
    export let S3URL: string;
    export let files: WalterS3File[];

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

    async function upload() {
        fileUploadComplete = false;
        for (const file of newFiles) {
            {
                walter_s3_post(file, S3URL, f).then(() =>
                    upload_finished(file)
                );
            }
        }
    }

    async function showModal(e: MouseEvent) {
        const fileName = (e!.target as any).textContent;
        walter_s3_get(`${S3URL}/${fileName}`).then((e: Blob) => {
            const file = new File([e], fileName, { type: e.type });
            selectedFile = create_walter_s3_file_from_file(file, S3URL);
            previewOpen = true;
        });
    }

    let selectedFile: WalterS3File;
    let previewOpen = false;
</script>

{#if selectedFile}
    <WalterPreview
        bind:files
        bind:file={selectedFile}
        bind:open={previewOpen}
    />
{/if}

<HeaderPanelLinks>
    <FileUploader
        status={fileUploadComplete ? 'complete' : 'uploading'}
        bind:files={newFiles}
        on:add={upload}
        multiple
        buttonLabel="Datei hochladen"
    />
    <HeaderPanelDivider>Dateien ({files.length})</HeaderPanelDivider>
    <HeaderPanelLinks>
        {#each files as file}
            <HeaderPanelLink on:click={showModal}>
                <!-- Copy the style from the original element. -->
                <Truncate
                    style="font-size: 0.875rem;
							   margin-left: 0;
							   font-weight: 600;
							   line-height: 1.28572;
							   letter-spacing: 0.16px;
							   display: block;
							   height: 2rem;
							   color: #c6c6c6;
							   text-decoration: none;"
                >
                    {file.FileName}
                </Truncate>
            </HeaderPanelLink>
        {/each}
    </HeaderPanelLinks>
</HeaderPanelLinks>
