<script lang="ts">
    import {
        HeaderPanelDivider,
        InlineLoading
    } from 'carbon-components-svelte';
    import { WalterAnhaengeEntry } from '..';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import type { WalterS3FileHandle } from '$walter/lib/WalterS3FileWrapper';

    export let permissions: WalterPermissions | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let allHandles: WalterS3FileHandle[];
    export let handle: WalterS3FileHandle;
    export let filter: string;
</script>

{#await handle.files}
    <HeaderPanelDivider>{handle.name} (-)</HeaderPanelDivider>
    <InlineLoading style="margin-left: 2em" description="Lade Dateien" />
{:then files}
    <HeaderPanelDivider>{handle.name} ({files?.length || 0})</HeaderPanelDivider
    >
    {#each files || [] as file}
        {#if file.FileName.toLowerCase().includes(filter.toLowerCase())}
            <WalterAnhaengeEntry
                bind:allHandles
                {permissions}
                {file}
                {fetchImpl}
                bind:handle
            />
        {/if}
    {/each}
{/await}
