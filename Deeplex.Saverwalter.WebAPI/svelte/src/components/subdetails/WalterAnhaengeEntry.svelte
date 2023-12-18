<script lang="ts">
    import type { WalterS3File } from '$walter/types';

    export let file: WalterS3File;
    export let fileWrapper: WalterS3FileWrapper;
    export let permissions: WalterPermissions | undefined;

    import {
        HeaderPanelLink,
        TooltipDefinition
    } from 'carbon-components-svelte';
    import WalterPreview from '../preview/WalterPreview.svelte';
    import type { WalterS3FileWrapper } from '$walter/lib';
    import { get_file_and_update_url } from './WalterAnhaengeEntry';
    import { page } from '$app/stores';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';

    async function showModal() {
        selectedFile = await get_file_and_update_url(file);
        previewOpen = true;
    }

    let selectedFile: WalterS3File;
    let previewOpen = false;

    const style = `
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        width: 14em;
        height: 2rem;
        color: #c6c6c6;
        text-align: left;
        padding-top: 7px`;
</script>

{#if selectedFile}
    <WalterPreview
        {permissions}
        bind:fileWrapper
        bind:file={selectedFile}
        bind:open={previewOpen}
    />
{/if}

<HeaderPanelLink
    style={file.FileName === new URL($page.url).searchParams.get('file') &&
        'background-color: #393939;'}
    on:click={showModal}
>
    <TooltipDefinition style="top: -7px;" align="start" direction="top">
        <svelte:fragment slot="tooltip">
            <div style="width: 14em;">
                {file.FileName}
            </div>
        </svelte:fragment>
        <p {style}>
            {file.FileName}
        </p>
    </TooltipDefinition>
</HeaderPanelLink>
