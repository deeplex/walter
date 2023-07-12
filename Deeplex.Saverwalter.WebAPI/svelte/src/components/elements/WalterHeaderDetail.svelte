<script lang="ts">
    import {
        HeaderAction,
        HeaderGlobalAction,
        HeaderPanelLink,
        HeaderPanelLinks,
        HeaderUtilities
    } from 'carbon-components-svelte';
    import { Save, TrashCan } from 'carbon-icons-svelte';

    import { WalterAnhaenge, WalterHeader } from '$walter/components';
    import { handle_save } from './WalterDataWrapper';
    import { handle_delete } from './WalterHeaderDetail';
    import type { WalterS3FileWrapper } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let title = 'Saverwalter';
    export let entry: any;
    export let apiURL: string;
    export let fileWrapper: WalterS3FileWrapper | undefined = undefined;

    let winWidth = 0;

    function click_save(e: MouseEvent): void {
        handle_save(apiURL, entry, title);
    }

    function click_delete(e: MouseEvent): void {
        handle_delete(title, apiURL);
    }
</script>

<svelte:window bind:innerWidth={winWidth} />

<WalterHeader bind:title>
    <HeaderUtilities>
        {#if winWidth < 1056}
            <HeaderAction>
                <HeaderPanelLinks>
                    <HeaderPanelLink on:click={click_save}
                        >Speichern</HeaderPanelLink
                    >
                    <HeaderPanelLink on:click={click_delete}
                        >Löschen</HeaderPanelLink
                    >
                </HeaderPanelLinks>
                {#if fileWrapper}
                    <div style="height: 1em; margin-top: 4em" />
                    <WalterAnhaenge bind:fileWrapper />
                {/if}
            </HeaderAction>
        {:else}
            <HeaderGlobalAction on:click={click_save} icon={Save} />
            <HeaderGlobalAction on:click={click_delete} icon={TrashCan} />

            {#if fileWrapper}
                {#await fileWrapper.handles[0].files}
                    <HeaderAction text="(...)">
                        <WalterAnhaenge bind:fileWrapper />
                    </HeaderAction>
                {:then files}
                    <HeaderAction text="({files.length})">
                        <WalterAnhaenge bind:fileWrapper />
                    </HeaderAction>
                {/await}
            {/if}
        {/if}
    </HeaderUtilities>
</WalterHeader>
