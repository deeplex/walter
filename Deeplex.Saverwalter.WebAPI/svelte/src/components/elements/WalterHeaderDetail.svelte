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
    import { walter_delete, walter_put } from '$walter/services/requests';
    import { openModal } from '$walter/store';
    import { WalterS3FileWrapper, WalterToastContent } from '$walter/lib';

    export let title = 'Saverwalter';
    export let entry: any;
    export let apiURL: string;
    export let fileWrapper: WalterS3FileWrapper | undefined = undefined;

    let winWidth = 0;

    const PutToast = new WalterToastContent(
        'Speichern erfolgreich',
        'Speichern fehlgeschlagen',
        (_a: unknown) => `${title} erfolgreich gespeichert.`,
        (a: any) =>
            `Folgende Einträge sind erforderlich:
			${Object.keys(a.errors)
                .map((e) => e.split('.').pop())
                .join(', \n')}`
    );

    function click_save() {
        walter_put(apiURL, entry, PutToast);
    }

    const DeleteToast = new WalterToastContent(
        'Löschen erfolgreich',
        'Löschen fehlgeschlagen',
        (_a: unknown) => `${title} erfolgreich gelöscht.`,
        (_a: unknown) => ''
    );

    function click_delete(title: string) {
        const content = `Bist du sicher, dass du ${title} löschen möchtest?
    	Dieser Vorgang kann nicht rückgängig gemacht werden.`;

        openModal({
            modalHeading: 'Löschen',
            content,
            danger: true,
            primaryButtonText: 'Löschen',
            submit: () =>
                walter_delete(apiURL, DeleteToast).then(() => history.back())
        });
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
                    <HeaderPanelLink on:click={() => click_delete(title)}
                        >Löschen</HeaderPanelLink
                    >
                </HeaderPanelLinks>
                {#if fileWrapper}
                    <div style="height: 1em" />
                    <WalterAnhaenge bind:fileWrapper />
                {/if}
            </HeaderAction>
        {:else}
            <p>{winWidth}</p>
            <HeaderGlobalAction on:click={click_save} icon={Save} />
            <HeaderGlobalAction
                on:click={() => click_delete(title)}
                icon={TrashCan}
            />

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
