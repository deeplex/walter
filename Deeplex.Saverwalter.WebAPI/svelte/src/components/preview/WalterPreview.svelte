<script lang="ts">
    import type { WalterFile } from '$walter/types';
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
    import { download } from './WalterPreview';
    import {
        WalterToastContent,
        type WalterSelectionEntry,
        WalterFileHandle
    } from '$walter/lib';
    import WalterPreviewType from './WalterPreviewDataTypeSelector.svelte';
    import { ArrowLeft, ArrowRight, Download } from 'carbon-icons-svelte';
    import {
        copyImpl,
        moveImpl,
        renameImpl,
        type WalterPreviewCopyTable
    } from './WalterPreviewCopyFile';
    import { get_file_and_update_url } from '../subdetails/WalterAnhaengeEntry';
    import WalterRenameFile from './WalterRenameFile.svelte';
    import type { WalterPermissions } from '$walter/lib/WalterPermissions';
    import { openModal } from '$walter/store';
    import { walter_file_delete } from '$walter/services/files';

    export let open = false;
    export let file: WalterFile;
    export let fetchImpl: typeof fetch;
    export let permissions: WalterPermissions | undefined;
    export let handle: WalterFileHandle;
    export let allHandles: WalterFileHandle[];

    // Created here to keep them when the selection changes
    let entry = {};
    let rows: WalterSelectionEntry[] | undefined = undefined;

    function close() {
        open = false;
    }

    function click_download(): void {
        download(file);
    }

    async function selectFileNextToSelectedFile(step: number) {
        const files = await handle.files;
        const fileIndex = files.findIndex((e) => e.key === file.key);

        const targetIndex = fileIndex + step;

        if (targetIndex < 0 || targetIndex >= files.length) {
            return;
        }

        const nextFile: WalterFile = files[targetIndex];
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

    let newFileName = file.fileName;

    function updateOtherHandle() {
        const otherHandleIndex = allHandles.findIndex(
            (e) => e.name === selectedEntry!.text
        );
        if (otherHandleIndex !== -1) {
            allHandles[otherHandleIndex].files =
                allHandles[otherHandleIndex].addFile(file);
        }
    }

    async function copy() {
        const copied = await copyImpl(
            file,
            fetchImpl,
            selectedTable,
            selectedEntry
        );

        if (copied && selectedTable && selectedEntry) {
            updateOtherHandle();
            selectedTable = undefined;
            selectedEntry = undefined;
            step = 0;
        }
    }

    async function move() {
        const moved = await moveImpl(
            file,
            fetchImpl,
            selectedTable,
            selectedEntry
        );

        if (moved && selectedTable && selectedEntry) {
            open = false;
            updateOtherHandle();
            handle.files = handle.removeFile(file);
        }
    }

    async function rename() {
        const renamed = await renameImpl(file, fetchImpl, newFileName);

        if (renamed) {
            handle.files = handle.files.then((files) => {
                const index = files.findIndex((e) => e.key === file.key);
                files[index].fileName = newFileName;
                files[index].key = `${file.key.substring(
                    0,
                    file.key.lastIndexOf('/') + 1
                )}${newFileName}`;
                return files;
            });
        }
    }

    async function remove(file: WalterFile) {
        const content = `Bist du sicher, dass du ${file.fileName} löschen möchtest?`;

        const deleteToast = new WalterToastContent(
            'Löschen erfolgreich',
            'Löschen fehlgeschlagen',
            () => `${file.fileName} erfolgreich gelöscht.`,
            () => ''
        );

        openModal({
            modalHeading: 'Löschen',
            content,
            danger: true,
            primaryButtonText: 'Löschen',
            submit: () =>
                walter_file_delete(file, deleteToast).then(async (e) => {
                    if (e.status === 200) {
                        handle.files = handle.removeFile(file);
                        open = false;
                    }
                })
        });
    }

    function submit() {
        switch (selectedTab) {
            case TabSelector.Copy:
                copy();
                break;
            case TabSelector.Move:
                move();
                break;
            case TabSelector.Rename:
                rename();
                break;
            case TabSelector.Delete:
                remove(file);
                break;
            default:
                break;
        }
    }

    enum TabSelector {
        Preview,
        Copy,
        Move,
        Rename,
        Delete
    }
</script>

<ComposedModal size="lg" bind:open on:submit>
    <ModalHeader bind:title={file.fileName}>
        <div style="display: flex; flex-direction: row">
            <Tabs bind:selected={selectedTab}>
                <Tab label="Vorschau" />
                <Tab label="Kopieren" />
                <Tab
                    label="Verschieben"
                    disabled={permissions && !permissions?.update}
                />
                <Tab
                    label="Umbenennen"
                    disabled={permissions && !permissions?.update}
                />
                <Tab
                    label="Löschen"
                    disabled={permissions && !permissions?.update}
                />
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
    <ModalBody>
        {#if selectedTab === TabSelector.Preview}
            <WalterPreviewType bind:file />
        {:else if selectedTab === TabSelector.Copy || selectedTab === TabSelector.Move}
            <WalterPreviewCopyFile
                bind:rows
                bind:entry
                bind:selectedTable
                bind:selectedEntry
                bind:step
                {fetchImpl}
            />
        {:else if selectedTab === TabSelector.Rename}
            <WalterRenameFile bind:value={newFileName} bind:file />
        {:else if selectedTab === TabSelector.Delete}
            <p>Datei {file.fileName} löschen</p>
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
