<script lang="ts">
    import { WalterPreviewError } from '$walter/components';
    import type { WalterFile } from '$walter/types';
    import { ImageLoader } from 'carbon-components-svelte';
    import { onDestroy, onMount } from 'svelte';

    export let file: WalterFile;

    let src: string;

    onDestroy(() => {
        URL.revokeObjectURL(src);
    });

    onMount(() => {
        if (file.blob) {
            src = URL.createObjectURL(file.blob);
        }
    });

    $: {
        src = file.blob ? URL.createObjectURL(file.blob) : '';
    }
</script>

{#if file.blob}
    <ImageLoader {src} />
{:else}
    <WalterPreviewError {file} />
{/if}
