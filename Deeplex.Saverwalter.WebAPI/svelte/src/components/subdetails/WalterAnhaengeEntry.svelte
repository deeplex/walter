<script lang="ts">
    import type { WalterS3File } from '$walter/types';

    export let file: WalterS3File;
    export let fileWrapper: WalterS3FileWrapper;

    import { HeaderPanelLink, TooltipDefinition, Truncate } from 'carbon-components-svelte';
    import WalterPreview from '../preview/WalterPreview.svelte';
    import type { WalterS3FileWrapper } from '$walter/lib';
    import { get_file } from './WalterAnhaengeEntry';

    async function showModal(e: MouseEvent) {
        selectedFile = await get_file(file);
        previewOpen = true;
    }

    let selectedFile: WalterS3File;
    let previewOpen = false;
</script>

{#if selectedFile}
    <WalterPreview
        bind:fileWrapper
        bind:file={selectedFile}
        bind:open={previewOpen}
    />
{/if}

<HeaderPanelLink on:click={showModal}>
    <!-- Copy the style from the original element. -->
    <TooltipDefinition tooltipText={file.FileName} align="start" direction="top">
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
    </TooltipDefinition>
</HeaderPanelLink>
