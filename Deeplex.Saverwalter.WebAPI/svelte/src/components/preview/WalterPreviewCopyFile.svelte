<script lang="ts">
    import type { WalterS3File } from '$walter/types';
    import { Grid } from 'carbon-components-svelte';
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

    export let fileWrapper: WalterS3FileWrapper;
    export let step: number;
    export let selectedTable: WalterPreviewCopyTable | undefined = undefined;
    export let selectedEntry: WalterSelectionEntry | undefined = undefined;
    export let entry: any;
    export let rows: WalterSelectionEntry[] | undefined = undefined;

    onMount(async () => {
        const entryList = $page.url.pathname.split('/').filter((e) => e);
        if (!entryList.length) {
            return;
        }

        const tableName = entryList[0];

        if (tableName === 'stack' || tableName === 'trash') {
            return;
        }
        
        if (entryList.length !== 2) {
            step = 0;
        }

        const id = entryList[1];
        selectedTable = tables.find((t) => t.key === tableName);

        if (selectedTable) {
            step = 1;
        } else {
            return;
        }

        rows = (await selectedTable!.fetch(fileWrapper.fetchImpl)) || [];
        selectEntryFromId(id);
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

    async function selectEntryFromId(id: string) {
        selectedEntry = rows?.find((row) => row.id === id);
        entry = await walter_get(
            `/api/${selectedTable?.key}/${selectedEntry?.id}`,
            fileWrapper.fetchImpl
        );
    }

    async function selectedEntry_change(e: CustomEvent<any>) {
        setTimeout(() => (step = 2), 0);
        await selectEntryFromId(e.detail.id);
    }
</script>

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
        bind:entry
        fetchImpl={fileWrapper.fetchImpl}
        {updateRows}
        {selectEntryFromId}
    />
    <WalterPreviewCopyFileStep3
        bind:selectedTable
        bind:selectedEntry
        bind:step
    />
</Grid>
