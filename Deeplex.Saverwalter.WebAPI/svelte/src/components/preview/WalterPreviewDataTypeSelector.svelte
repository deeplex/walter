<script lang="ts">
    import type { WalterS3File } from '$walter/types';
    import { WalterPreviewPdf, WalterPreviewUnknown } from '..';

    import WalterPreviewImage from './WalterPreviewImage.svelte';
    import WalterPreviewText from './WalterPreviewText.svelte';

    export let file: WalterS3File;
</script>

{#if file.Type?.includes('image/')}
    <WalterPreviewImage bind:file />
{:else if file.Type === 'text/plain' || file.Type === 'application/json'}
    <WalterPreviewText bind:file />
{:else if file.Type === 'application/pdf'}
    <WalterPreviewPdf bind:file />
{:else}
    <WalterPreviewUnknown bind:file />
{/if}
