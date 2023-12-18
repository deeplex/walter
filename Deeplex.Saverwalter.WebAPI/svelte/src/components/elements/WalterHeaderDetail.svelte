<script lang="ts">
    import {
        HeaderAction,
        HeaderGlobalAction,
        HeaderPanelLink,
        HeaderPanelLinks,
        HeaderUtilities,
        Tooltip
    } from 'carbon-components-svelte';
    import { Save, TrashCan } from 'carbon-icons-svelte';

    import { WalterAnhaenge, WalterHeader } from '$walter/components';
    import { handle_save, handle_delete } from './WalterHeaderDetail';
    import type { WalterS3FileWrapper } from '$walter/lib';
    import { convertTime } from '$walter/services/utils';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';

    export let title: string;
    export let entry: {
        createdAt: Date;
        lastModified: Date;
        permissions?: WalterPermissions;
    } & unknown;
    export let apiURL: string;
    export let fileWrapper: WalterS3FileWrapper | undefined = undefined;

    let winWidth = 0;

    function click_save(): void {
        handle_save(apiURL, entry, title);
    }

    function click_delete(): void {
        handle_delete(title, apiURL);
    }

    let isOpen = false;
</script>

<svelte:window bind:innerWidth={winWidth} />

<WalterHeader bind:title>
    <HeaderUtilities>
        {#if winWidth < 1056}
            <HeaderAction preventCloseOnClickOutside>
                <HeaderPanelLinks>
                    {#if entry?.permissions?.update}
                        <HeaderPanelLink on:click={click_save}
                            >Speichern</HeaderPanelLink
                        >
                    {/if}
                    {#if entry?.permissions?.remove}
                        <HeaderPanelLink on:click={click_delete}
                            >Löschen</HeaderPanelLink
                        >
                    {/if}
                </HeaderPanelLinks>
                {#if fileWrapper}
                    <div style="height: 1em; margin-top: 4em" />
                    <WalterAnhaenge
                        permissions={entry.permissions}
                        bind:fileWrapper
                    />
                {/if}
            </HeaderAction>
        {:else}
            {#if entry?.permissions?.update}
                <HeaderGlobalAction on:click={click_save} icon={Save} />
            {/if}
            {#if entry?.permissions?.remove}
                <HeaderGlobalAction on:click={click_delete} icon={TrashCan} />
            {/if}

            {#if fileWrapper}
                {#await fileWrapper.handles[0].files}
                    <HeaderAction
                        {isOpen}
                        preventCloseOnClickOutside
                        text="(...)"
                    >
                        <WalterAnhaenge
                            permissions={entry.permissions}
                            bind:fileWrapper
                        />
                    </HeaderAction>
                {:then files}
                    <HeaderAction
                        {isOpen}
                        preventCloseOnClickOutside
                        text="({files.length})"
                    >
                        <WalterAnhaenge
                            permissions={entry.permissions}
                            bind:fileWrapper
                        />
                    </HeaderAction>
                {/await}
            {/if}
        {/if}
    </HeaderUtilities>

    <Tooltip
        direction="top"
        align="end"
        style="position: absolute; right: 0.75vw; bottom: -96.5vh; text-align: end;"
    >
        <p>Erstellt am:</p>
        <p>{convertTime(entry.createdAt)}</p>
        <p>Zuletzt geändert am:</p>
        <p>{convertTime(entry.lastModified)}</p>
    </Tooltip>
</WalterHeader>
