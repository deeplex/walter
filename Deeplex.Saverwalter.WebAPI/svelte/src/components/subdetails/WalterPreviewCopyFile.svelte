<script lang="ts">
    import { WalterToastContent, type WalterSelectionEntry } from '$walter/lib';
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
        tables,
        type WalterPreviewCopyTable
    } from './WalterPreviewCopyFile';
    import { walter_s3_post } from '$walter/services/s3';
    import { WalterDataTable } from '..';
    import { page } from '$app/stores';

    export let file: WalterS3File;
    export let fetchImpl: typeof fetch;

    export let open = false;
    let rows: WalterSelectionEntry[] | undefined = undefined;

    function close() {
        open = false;
    }

    let selectedTable: WalterPreviewCopyTable | undefined = undefined;
    let selectedEntry: WalterSelectionEntry | undefined = undefined;

    const headers = [{ key: 'text', value: 'Bezeichnung' }];

    async function selectedTable_change(e: any) {
        selectedTable = tables.find((t) => t.key === e.target.value);
        rows = undefined;
        step = 1;
        rows = (await selectedTable!.fetch(fetchImpl)) || [];
    }

    async function copy() {
        if (!file.Blob) {
            return;
        }

        if (!selectedTable) {
            return;
        }
        const S3URL = `${selectedTable.S3URL}/${selectedEntry?.id}`;

        const copyToast = new WalterToastContent(
            'Kopieren erfolgreich',
            'Kopieren fehlgeschlagen',
            () =>
                `Die Datei ${file.FileName} wurde erfolgreich zu ${
                    rows!.find((row) => row.id === selectedEntry!.id)?.text
                } kopiert.`,
            () =>
                `Die Datei ${file.FileName} konnte nicht zu ${
                    rows!.find((row) => row.id === selectedEntry!.id)?.text
                } kopiert werden.`
        );

        const success = walter_s3_post(
            new File([file.Blob], file.FileName),
            S3URL,
            fetchImpl,
            copyToast
        );

        if ((await success).ok) {
            open = false;
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

    console.log(`${$page.url.origin}`);

    let step = 0;
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
        <Button disabled={!selectedEntry} kind="tertiary" on:click={copy}
            >Kopieren</Button
        >
    </ModalFooter>
</ComposedModal>
