<script lang="ts">
    import type { WalterFile } from '$walter/types';
    import { WalterPreviewPdf, WalterPreviewUnknown } from '..';

    import WalterPreviewImage from './WalterPreviewImage.svelte';
    import WalterPreviewText from './WalterPreviewText.svelte';

    export let file: WalterFile;
</script>

{#if file.type?.includes('image/')}
    <WalterPreviewImage bind:file />
{:else if file.type === 'text/plain' || file.type === 'application/json'}
    <WalterPreviewText bind:file />
{:else if file.type === 'application/pdf'}
    <WalterPreviewPdf bind:file />
{:else}
    <WalterPreviewUnknown bind:file />
{/if}
