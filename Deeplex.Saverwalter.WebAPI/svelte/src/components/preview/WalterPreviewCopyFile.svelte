<script lang="ts">
    import { Grid } from 'carbon-components-svelte';
    import {
        tables,
        type WalterPreviewCopyTable
    } from './WalterPreviewCopyFile';
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import type {
        WalterS3FileWrapper,
        WalterSelectionEntry
    } from '$walter/lib';
    import Step0SelectTable from './WalterStep0SelectTable.svelte';
    import Step1SelectEntry from './WalterStep1SelectEntry.svelte';
    import Step2ViewOrCreateEntry from './WalterStep2ViewOrCreateEntry.svelte';
    import Step3CheckAndSubmit from './WalterStep3CheckAndSubmit.svelte';
    import WalterPreviewCopyFileStepper from './WalterStepper.svelte';
    import { walter_get } from '$walter/services/requests';

    export let fileWrapper: WalterS3FileWrapper;
    export let step: number;
    export let selectedTable: WalterPreviewCopyTable | undefined = undefined;
    export let selectedEntry: WalterSelectionEntry | undefined = undefined;
    export let entry: unknown;
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

    async function selectedTable_change(e: Event) {
        selectedTable = tables.find(
            (t) => t.key === (e.target as HTMLSelectElement)?.value
        );
        rows = undefined;
        if (selectedTable?.key === 'stack') {
            step = 3;
        } else {
            step = 1;
            updateRows();
        }
    }

    async function selectEntryFromId(id: string) {
        selectedEntry = rows?.find((row) => row.id === id);
        entry = await walter_get(
            `${selectedTable?.S3URL(`${selectedEntry?.id}`)}`,
            fileWrapper.fetchImpl
        );
    }

    async function selectedEntry_change(e: CustomEvent) {
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
    <Step0SelectTable
        bind:selectedTable
        bind:step
        {selectedTable_change}
        {tables}
    />
    <Step1SelectEntry
        bind:entry
        bind:step
        bind:selectedEntry
        {rows}
        {selectedEntry_change}
    />
    <Step2ViewOrCreateEntry
        bind:step
        bind:selectedEntry
        bind:selectedTable
        bind:entry
        fetchImpl={fileWrapper.fetchImpl}
        {updateRows}
        {selectEntryFromId}
    />
    <Step3CheckAndSubmit bind:selectedTable bind:selectedEntry bind:step />
</Grid>
