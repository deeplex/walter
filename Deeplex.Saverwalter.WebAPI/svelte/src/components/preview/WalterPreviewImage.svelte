<script lang="ts">
    import { WalterPreviewError } from '$walter/components';
    import type { WalterS3File } from '$walter/types';
    import { ImageLoader } from 'carbon-components-svelte';
    import { onDestroy, onMount } from 'svelte';

    export let file: WalterS3File;

    let src: string;

    onDestroy(() => {
        URL.revokeObjectURL(src);
    });

    onMount(() => {
        if (file.Blob) {
            src = URL.createObjectURL(file.Blob);
        }
    });
</script>

{#if file.Blob}
    <ImageLoader {src} />
{:else}
    <WalterPreviewError {file} />
{/if}
