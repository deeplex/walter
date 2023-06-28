<script lang="ts">
    import type { WalterS3File } from '$walter/types';
    import {
        Button,
        ComposedModal,
        DataTableSkeleton,
        Grid,
        Link,
        ModalBody,
        ModalFooter,
        ModalHeader,
        ProgressIndicator,
        ProgressStep,
        RadioButton,
        RadioButtonGroup,
        SkeletonText,
        Tile
    } from 'carbon-components-svelte';
    import {
        copyImpl,
        moveImpl,
        tables,
        type WalterPreviewCopyTable
    } from './WalterPreviewCopyFile';
    import { WalterDataTable } from '..';
    import { page } from '$app/stores';
    import { onMount } from 'svelte';
    import type {
        WalterS3FileWrapper,
        WalterSelectionEntry
    } from '$walter/lib';

    export let file: WalterS3File;
    export let fileWrapper: WalterS3FileWrapper;

    export let open = false;
    let rows: WalterSelectionEntry[] | undefined = undefined;

    const headers = [{ key: 'text', value: 'Bezeichnung' }];
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
        const copied = await copyImpl(
            file,
            selectedTable!,
            selectedEntry!,
            fileWrapper.fetchImpl
        );

        if (copied) {
            open = false;
            // TODO replace
            // files = files.filter(
            //     (e: WalterS3File) => e.FileName !== file.FileName
            // );
        }
    }

    async function move() {
        const moved = await moveImpl(
            file,
            selectedTable!,
            selectedEntry!,
            fileWrapper.fetchImpl
        );

        if (moved) {
            open = false;
            // TODO replace
            // files = files.filter(
            //     (e: WalterS3File) => e.FileName !== file.FileName
            // );
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
            <ProgressIndicator
                style="margin: 1em"
                spaceEqually
                currentIndex={step}
            >
                <ProgressStep
                    on:click={() => (step = 0)}
                    label="Tabelle auswählen"
                    complete={!!selectedTable}
                />
                <ProgressStep
                    on:click={() => {
                        if (!!selectedTable) step = 1;
                    }}
                    label="Eintrag auswählen"
                    complete={step > 1}
                />
                <ProgressStep
                    on:click={() => {
                        if (selectedEntry) step = 2;
                    }}
                    label="Bestätigen"
                    complete={step > 2}
                />
            </ProgressIndicator>
            <style>
                .bx--progress-label {
                    padding-right: 0px !important;
                }
            </style>
            {#if step === 0}
                <RadioButtonGroup orientation="vertical">
                    {#each tables as radio}
                        <RadioButton
                            checked={selectedTable?.key == radio.key}
                            on:change={selectedTable_change}
                            labelText={radio.value}
                            value={radio.key}
                        />
                    {/each}
                </RadioButtonGroup>
            {:else if step === 1}
                {#if rows}
                    <WalterDataTable
                        fullHeight
                        navigate={selectedEntry_change}
                        {headers}
                        {rows}
                        search
                    />
                {:else}
                    <SkeletonText style="margin: 0; height: 48px" />
                    <DataTableSkeleton
                        {headers}
                        showHeader={false}
                        showToolbar={false}
                    />
                {/if}
            {:else if step === 2}
                <Tile light>
                    Ausgewählter Eintrag von {selectedTable?.value}: <Link
                        href="{$page.url
                            .origin}/{selectedTable?.key}/{selectedEntry?.id}"
                        >{selectedEntry?.text}</Link
                    >
                </Tile>
                <Tile light>Datei kann jetzt kopiert werden.</Tile>
            {/if}
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
