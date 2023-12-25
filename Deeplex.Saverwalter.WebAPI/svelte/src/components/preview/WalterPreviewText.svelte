<script lang="ts">
    import { WalterPreviewError } from '$walter/components';
    import type { WalterFile } from '$walter/types';
    import { Tile } from 'carbon-components-svelte';
    import { onMount } from 'svelte';

    export let file: WalterFile;
    let text = '';

    onMount(() => {
        const reader = new FileReader();
        reader.onload = function (event) {
            text = (event.target?.result as string) || '';
        };
        if (file.blob) {
            reader.readAsText(file.blob);
        }
    });
</script>

{#if file.blob}
    <Tile light>{text}</Tile>
{:else}
    <WalterPreviewError {file} />
{/if}
