<script lang="ts">
    import { WalterPreviewError } from '$walter/components';
    import type { WalterS3File } from '$walter/types';
    import { Tile } from 'carbon-components-svelte';
    import { onMount } from 'svelte';

    export let file: WalterS3File;
    let text = '';

    onMount(() => {
        const reader = new FileReader();
        reader.onload = function (event) {
            text = (event.target?.result as string) || '';
        };
        if (file.Blob) {
            reader.readAsText(file.Blob);
        }
    });
</script>

{#if file.Blob}
    <Tile light>{text}</Tile>
{:else}
    <WalterPreviewError {file} />
{/if}
