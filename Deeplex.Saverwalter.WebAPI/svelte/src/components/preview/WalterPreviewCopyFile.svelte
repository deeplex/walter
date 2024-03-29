<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import { Grid } from 'carbon-components-svelte';
    import {
        tables,
        type WalterPreviewCopyTable
    } from './WalterPreviewCopyFile';
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import Step0SelectTable from './WalterStep0SelectTable.svelte';
    import Step1SelectEntry from './WalterStep1SelectEntry.svelte';
    import Step2ViewOrCreateEntry from './WalterStep2ViewOrCreateEntry.svelte';
    import Step3CheckAndSubmit from './WalterStep3CheckAndSubmit.svelte';
    import WalterPreviewCopyFileStepper from './WalterStepper.svelte';
    import { walter_get } from '$walter/services/requests';
    import type { WalterSelectionEntry } from '$walter/lib';

    export let fetchImpl: typeof fetch;
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

        rows =
            ((await selectedTable!.fetch(
                fetchImpl
            )) as WalterSelectionEntry[]) || [];
        selectEntryFromId(id);
        if (selectedEntry) {
            step = 2;
        }
    });

    async function updateRows() {
        rows =
            ((await selectedTable!.fetch(
                fetchImpl
            )) as WalterSelectionEntry[]) || [];
    }

    async function selectedTable_change(key: string) {
        selectedTable = tables.find((t) => t.key === key);
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
            `${selectedTable?.ApiURL}/${selectedEntry?.id}`,
            fetchImpl
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
    <Step0SelectTable bind:step {selectedTable_change} {tables} />
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
        {fetchImpl}
        {updateRows}
        {selectEntryFromId}
    />
    <Step3CheckAndSubmit bind:selectedTable bind:selectedEntry bind:step />
</Grid>
