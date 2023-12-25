<script lang="ts">
    import { onDestroy, onMount } from 'svelte';
    import PDFObject from 'pdfobject';
    import type { WalterFile } from '$walter/types';
    import { WalterPreviewError } from '$walter/components';

    export let file: WalterFile;

    let src: string;
    onDestroy(() => {
        URL.revokeObjectURL(src);
    });

    onMount(() => {
        if (file.blob) {
            src = URL.createObjectURL(file.blob);
            PDFObject.embed(src, '#pdf-container');
        }
    });
</script>

{#if file.blob}
    <div style="height:50rem; overflow: hidden" id="pdf-container" />
{:else}
    <WalterPreviewError {file} />
{/if}
