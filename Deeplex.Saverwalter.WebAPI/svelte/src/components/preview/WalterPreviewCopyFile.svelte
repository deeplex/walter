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
    import WalterPreviewCopyFileStep3 from './WalterPreviewCopyFileStep3.svelte';
    import WalterPreviewCopyFileStepper from './WalterPreviewCopyFileStepper.svelte';
    import { walter_get } from '$walter/services/requests';

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

    async function updateRows() {
        rows = (await selectedTable!.fetch(fileWrapper.fetchImpl)) || [];
    }

    async function selectedTable_change(e: any) {
        selectedTable = tables.find((t) => t.key === e.target.value);
        rows = undefined;
        step = 1;
        updateRows();
    }

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

    let value = {};

    function selectEntryFromId(id: string) {
        selectedEntry = rows?.find((row) => row.id === id);
    }

    async function selectedEntry_change(e: CustomEvent<any>) {
        // e.stopImmediatePropagation();
        // e.preventDefault();
        // e.stopPropagation();
        // Is this the best way of stopping the modal to stop? ...
        selectEntryFromId(e.detail.id);
        setTimeout(() => (step = 3), 0);
        value = await walter_get(
            `/api/${selectedTable?.key}/${selectedEntry?.id}`,
            fileWrapper.fetchImpl
        );
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
                bind:selectedEntry
                {rows}
                {selectedEntry_change}
            />
            <WalterPreviewCopyFileStep2
                bind:step
                bind:selectedEntry
                bind:selectedTable
                bind:value
                {updateRows}
                {selectEntryFromId}
            />
            <WalterPreviewCopyFileStep3
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
