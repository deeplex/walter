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
    import { Download } from 'carbon-icons-svelte';
    import {
        copyImpl,
        moveImpl,
        type WalterPreviewCopyTable
    } from './WalterPreviewCopyFile';

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
            open = false;

            fileWrapper.addFile(
                file,
                `${selectedTable.key}/${selectedEntry.id}`
            );
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
                on:click={click_download}
                style="top: -3em"
                kind="tertiary"
                iconDescription="Herunterladen"
                icon={Download}
            />
        </div>
    </ModalHeader>
    <ModalBody>
        {#if selectedTab === TabSelector.Preview}
            <WalterPreviewType {file} />
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
            <p>Datei {file.FileName} unwiderruflich löschen</p>
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
