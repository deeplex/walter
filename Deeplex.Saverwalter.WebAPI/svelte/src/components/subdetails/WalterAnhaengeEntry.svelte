<script lang="ts">
    import {
        create_walter_s3_file_from_file,
        walter_s3_get
    } from '$walter/services/s3';
    import type { WalterS3File } from '$walter/types';

    export let S3URL: string;
    export let fetchImpl: typeof fetch;
    export let file: WalterS3File;
    export let files: WalterS3File[];

    import { HeaderPanelLink, Truncate } from 'carbon-components-svelte';
    import WalterPreview from './WalterPreview.svelte';

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
        {fetchImpl}
        bind:files
        bind:file={selectedFile}
        bind:open={previewOpen}
    />
{/if}

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
