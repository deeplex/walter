<script lang="ts">
    import {
        WalterPreviewImage,
        WalterPreviewPdf,
        WalterPreviewText,
        WalterPreviewUnknown
    } from '$walter/components';
    import { download_file_blob, walter_s3_delete } from '$walter/services/s3';
    import { openModal } from '$walter/store';
    import type { WalterS3File } from '$walter/types';
    import {
        Button,
        ComposedModal,
        ModalBody,
        ModalFooter,
        ModalHeader
    } from 'carbon-components-svelte';

    export let open = false;
    export let file: WalterS3File;
    export let files: WalterS3File[];
    // export let fullscreen: boolean = false;

    function close() {
        open = false;
    }

    function download() {
        if (file.Blob) {
            download_file_blob(file.Blob, file.FileName);
        }
    }

    function remove() {
        const content = `Bist du sicher, dass du ${file.FileName} löschen möchtest?
    	Dieser Vorgang kann nicht rückgängig gemacht werden.`;

        openModal({
            modalHeading: 'Löschen',
            content,
            danger: true,
            primaryButtonText: 'Löschen',
            submit: () =>
                walter_s3_delete(file).then(() => {
                    open = false;
                    files = files.filter((e) => e.FileName !== file.FileName);
                })
        });
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
        <Button kind="danger" on:click={remove}>Löschen</Button>
        <Button kind="primary" on:click={download}>Herunterladen</Button>
    </ModalFooter>
</ComposedModal>
