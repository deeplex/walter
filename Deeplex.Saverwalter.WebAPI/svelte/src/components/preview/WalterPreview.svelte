<script lang="ts">
    import {
        WalterPreviewImage,
        WalterPreviewPdf,
        WalterPreviewText,
        WalterPreviewUnknown
    } from '$walter/components';
    import type { WalterS3File } from '$walter/types';
    import {
        Button,
        ComposedModal,
        ModalBody,
        ModalFooter,
        ModalHeader
    } from 'carbon-components-svelte';
    import WalterPreviewCopyFile from '../preview/WalterPreviewCopyFile.svelte';
    import { download, remove } from '../preview/WalterPreview';
    import type { WalterS3FileWrapper } from '$walter/lib';

    export let open = false;
    export let file: WalterS3File;
    export let fileWrapper: WalterS3FileWrapper;

    function close() {
        open = false;
    }

    let copying = false;
    function copy() {
        copying = true;
    }

    function click_remove(e: MouseEvent): void {
        remove(file, fileWrapper);
    }

    function click_download(e: MouseEvent): void {
        download(file);
    }
</script>

<ComposedModal size="lg" bind:open on:submit>
    <ModalHeader bind:title={file.FileName} />
    <ModalBody>
        {#if file.Type?.includes('image/')}
            <WalterPreviewImage {file} />
        {:else if file.Type === 'text/plain' || file.Type === 'application/json'}
            <WalterPreviewText {file} />
        {:else if file.Type === 'application/pdf'}
            <WalterPreviewPdf {file} />
        {:else}
            <WalterPreviewUnknown {file} />
        {/if}
    </ModalBody>
    <ModalFooter>
        <Button kind="secondary" on:click={close}>Abbrechen</Button>
        <Button kind="tertiary" on:click={copy}>Kopieren / Verschieben</Button>
        <Button kind="danger" on:click={click_remove}>Löschen</Button>
        <Button kind="primary" on:click={click_download}>Herunterladen</Button>
    </ModalFooter>
</ComposedModal>

<WalterPreviewCopyFile bind:fileWrapper bind:open={copying} {file} />
