<script lang="ts">
    import type { WalterS3File } from '$walter/types';
    import {
        Button,
        ComposedModal,
        ModalBody,
        ModalFooter,
        ModalHeader,
        Tab,
        Tabs
    } from 'carbon-components-svelte';
    import WalterPreviewCopyFile from './WalterPreviewCopyFile.svelte';
    import { download, remove } from './WalterPreview';
    import type {
        WalterS3FileWrapper,
        WalterSelectionEntry
    } from '$walter/lib';
    import WalterPreviewType from './WalterPreviewDataTypeSelector.svelte';
    import { ArrowLeft, ArrowRight, Download } from 'carbon-icons-svelte';
    import {
        copyImpl,
        moveImpl,
        type WalterPreviewCopyTable
    } from './WalterPreviewCopyFile';
    import { get_file_and_update_url } from '../subdetails/WalterAnhaengeEntry';

    export let open = false;
    export let file: WalterS3File;
    export let fileWrapper: WalterS3FileWrapper;

    // Created here to keep them when the selection changes
    let entry = {};
    let rows: WalterSelectionEntry[] | undefined = undefined;

    function close() {
        open = false;
    }

    function click_download(e: MouseEvent): void {
        download(file);
    }

    async function selectFileNextToSelectedFile(step: number) {
        const handleIndex = fileWrapper.handles.findIndex(e => e.S3URL === file?.Key.slice(0, e.S3URL.length));
        const handle = fileWrapper.handles[handleIndex];
        const files = await handle.files;
        const fileIndex = files.findIndex(e => e.Key === file.Key);

        const targetIndex = fileIndex + step

        if (targetIndex < 0 || targetIndex >= files.length) {
            return;
        }

        const nextFile: WalterS3File = files[targetIndex];
        // First to update the fileName etc, then to load the blob
        file = nextFile;
        file = await get_file_and_update_url(nextFile);
    }

    async function fileBefore() {
        selectFileNextToSelectedFile(-1);
    }

    function fileAfter() {
        selectFileNextToSelectedFile(1);
    }

    let selectedTab = 0;

    let step = 0;
    let selectedTable: WalterPreviewCopyTable | undefined = undefined;
    let selectedEntry: WalterSelectionEntry | undefined = undefined;

    async function copy() {
        const copied = await copyImpl(
            file,
            fileWrapper.fetchImpl,
            selectedTable,
            selectedEntry
        );

        if (copied && selectedTable && selectedEntry) {
            fileWrapper.addFile(
                file,
                `${selectedTable.key}/${selectedEntry.id}`
            );
            selectedTable = undefined;
            selectedEntry = undefined;
            step = 0;
        }
    }

    async function move() {
        const moved = await moveImpl(
            file,
            fileWrapper.fetchImpl,
            selectedTable,
            selectedEntry
        );

        if (moved && selectedTable && selectedEntry) {
            open = false;
            fileWrapper.addFile(
                file,
                `${selectedTable.key}/${selectedEntry.id}`
            );
            fileWrapper.removeFile(file);
        }
    }

    function submit() {
        switch (selectedTab) {
            case TabSelector.Copy:
                copy();
                break;
            case TabSelector.Move:
                move();
                break;
            case TabSelector.Delete:
                remove(file, fileWrapper);
                break;
            default:
                break;
        }
    }

    enum TabSelector {
        Preview,
        Copy,
        Move,
        Delete
    }
</script>

<ComposedModal size="lg" bind:open on:submit>
    <ModalHeader bind:title={file.FileName}>
        <div style="display: flex; flex-direction: row">
            <Tabs bind:selected={selectedTab}>
                <Tab label="Vorschau" />
                <Tab label="Kopieren" />
                <Tab label="Verschieben" />
                <Tab label="Löschen" />
            </Tabs>
            <Button
                on:click={fileBefore}
                style="top: -3.15em"
                kind="tertiary"
                iconDescription="Vorherige Datei"
                icon={ArrowLeft}
            />
            <Button
                on:click={fileAfter}
                style="top: -3.15em"
                kind="tertiary"
                iconDescription="Nachfolgende Datei"
                icon={ArrowRight}
            />
            <Button
                on:click={click_download}
                style="top: -3.15em"
                kind="tertiary"
                iconDescription="Herunterladen"
                icon={Download}
            />
        </div>
    </ModalHeader>
    <ModalBody  style="min-height: 40em;">
        {#if selectedTab === TabSelector.Preview}
            <WalterPreviewType bind:file />
        {:else if selectedTab === TabSelector.Copy || selectedTab === TabSelector.Move}
            <WalterPreviewCopyFile
                bind:rows
                bind:entry
                bind:selectedTable
                bind:selectedEntry
                bind:step
                {fileWrapper}
            />
        {:else if selectedTab === TabSelector.Delete}
            <p>Datei {file.FileName} löschen</p>
        {/if}
    </ModalBody>
    {#if selectedTab !== TabSelector.Preview}
        <ModalFooter>
            <Button kind="secondary" on:click={close}>Abbrechen</Button>
            <Button
                disabled={(selectedTab === TabSelector.Copy ||
                    selectedTab === TabSelector.Move) &&
                    step < 3}
                kind={selectedTab === TabSelector.Delete ? 'danger' : 'primary'}
                on:click={submit}>Bestätigen</Button
            >
        </ModalFooter>
    {/if}
</ComposedModal>
