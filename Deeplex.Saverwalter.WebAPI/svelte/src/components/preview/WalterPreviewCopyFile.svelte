<script lang="ts">
    import type { WalterS3File } from '$walter/types';
    import {
        Button,
        ComposedModal,
        Grid,
        ModalBody,
        ModalFooter,
        ModalHeader
    } from 'carbon-components-svelte';
    import {
        copyImpl,
        moveImpl,
        tables,
        type WalterPreviewCopyTable
    } from './WalterPreviewCopyFile';
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import type {
        WalterS3FileWrapper,
        WalterSelectionEntry
    } from '$walter/lib';
    import WalterPreviewCopyFileStep0 from './WalterPreviewCopyFileStep0.svelte';
    import WalterPreviewCopyFileStep1 from './WalterPreviewCopyFileStep1.svelte';
    import WalterPreviewCopyFileStep2 from './WalterPreviewCopyFileStep2.svelte';
    import WalterPreviewCopyFileStepper from './WalterPreviewCopyFileStepper.svelte';

    export let file: WalterS3File;
    export let fileWrapper: WalterS3FileWrapper;

    export let open = false;
    let rows: WalterSelectionEntry[] | undefined = undefined;

    let step = 0;
    let selectedTable: WalterPreviewCopyTable | undefined = undefined;
    let selectedEntry: WalterSelectionEntry | undefined = undefined;

    function close() {
        open = false;
    }

    onMount(async () => {
        const entryList = $page.url.pathname.split('/').filter((e) => e);
        if (entryList.length !== 2) {
            step = 0;
        }

        const tableName = entryList[0];
        const id = entryList[1];
        selectedTable = tables.find((t) => t.key === tableName);

        if (selectedTable) {
            step = 1;
        } else {
            return;
        }

        rows = (await selectedTable!.fetch(fileWrapper.fetchImpl)) || [];
        selectedEntry = rows?.find((row) => row.id === id);
        if (selectedEntry) {
            step = 2;
        }
    });

    async function selectedTable_change(e: any) {
        selectedTable = tables.find((t) => t.key === e.target.value);
        rows = undefined;
        step = 1;
        rows = (await selectedTable!.fetch(fileWrapper.fetchImpl)) || [];
    }

    async function copy() {
        if (!selectedTable || !selectedEntry) {
            return;
        }

        const copied = await copyImpl(
            file,
            selectedTable,
            selectedEntry,
            fileWrapper.fetchImpl
        );

        if (copied) {
            open = false;

            fileWrapper.addFile(
                file,
                `${selectedTable.key}/${selectedEntry.id}`
            );
        }
    }

    async function move() {
        if (!selectedTable || !selectedEntry) {
            return;
        }

        const moved = await moveImpl(
            file,
            selectedTable,
            selectedEntry,
            fileWrapper.fetchImpl
        );

        if (moved) {
            open = false;
            fileWrapper.addFile(
                file,
                `${selectedTable.key}/${selectedEntry.id}`
            );
            fileWrapper.removeFile(file);
        }
    }

    function selectedEntry_change(e: CustomEvent<any>) {
        // e.stopImmediatePropagation();
        // e.preventDefault();
        // e.stopPropagation();
        // Is this the best way of stopping the modal to stop? ...
        selectedEntry = rows?.find((row) => row.id === e.detail.id);
        setTimeout(() => (step = 2), 0);
        // step = 2;
    }
</script>

<ComposedModal size="lg" bind:open>
    <ModalHeader title={`${file.FileName} kopieren`} />
    <ModalBody>
        <Grid condensed fullWidth>
            <WalterPreviewCopyFileStepper
                bind:step
                bind:selectedEntry
                bind:selectedTable
            />
            <style>
                .bx--progress-label {
                    padding-right: 0px !important;
                }
            </style>
            <WalterPreviewCopyFileStep0
                bind:selectedTable
                bind:step
                {selectedTable_change}
                {tables}
            />
            <WalterPreviewCopyFileStep1
                bind:step
                {rows}
                {selectedEntry_change}
            />
            <WalterPreviewCopyFileStep2
                bind:selectedTable
                bind:selectedEntry
                bind:step
            />
        </Grid>
    </ModalBody>
    <ModalFooter>
        <Button kind="secondary" on:click={close}>Abbrechen</Button>
        <Button disabled={!selectedEntry} kind="tertiary" on:click={move}
            >Verschieben</Button
        >
        <Button disabled={!selectedEntry} kind="tertiary" on:click={copy}
            >Kopieren</Button
        >
    </ModalFooter>
</ComposedModal>
