<script lang="ts">
    import { onDestroy, onMount } from 'svelte';
    import PDFObject from 'pdfobject';
    import type { WalterS3File } from '$walter/types';
    import { WalterPreviewError } from '$walter/components';

    export let file: WalterS3File;

    let src: string;
    onDestroy(() => {
        URL.revokeObjectURL(src);
    });

    onMount(() => {
        if (file.Blob) {
            src = URL.createObjectURL(file.Blob);
            PDFObject.embed(src, '#pdf-container');
        }
    });
</script>

{#if file.Blob}
    <div style="height:100vh" id="pdf-container" />
{:else}
    <WalterPreviewError {file} />
{/if}
