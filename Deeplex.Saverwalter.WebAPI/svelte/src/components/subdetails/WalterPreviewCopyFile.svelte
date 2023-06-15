<script lang="ts">
    import type { WalterSelectionEntry } from '$walter/lib';
    import type { WalterS3File } from '$walter/types';
    import {
        Button,
        Column,
        ComposedModal,
        DataTable,
        Grid,
        ModalBody,
        ModalFooter,
        ModalHeader,
        RadioButton,
        RadioButtonGroup,
        Row
    } from 'carbon-components-svelte';
    import { tables } from './WalterPreviewCopyFile';
    import { walter_s3_post } from '$walter/services/s3';

    export let file: WalterS3File;
    export let fetchImpl: typeof fetch;

    export let open = false;
    let rows: WalterSelectionEntry[] = [];

    function close() {
        open = false;
    }

    let selectedTable: string | undefined = undefined;
    let selectedRowIds: number[] = [];

    const headers = [{ key: 'text', value: 'Bezeichnung' }];

    async function radio_change() {
        const selection = tables.find((e) => e.key === selectedTable);
        rows = (await selection!.fetch(fetchImpl)) || [];
        console.log(rows);
    }

    async function copy() {
        if (!file.Blob) {
            return;
        }
        const selectedTableObject = tables.find(
            (e) => e.key === selectedTable
        ) as any;
        if (!selectedTableObject) {
            return;
        }
        const S3URL = `${selectedTableObject.S3URL}/${selectedRowIds[0]}`;
        console.log(
            walter_s3_post(
                new File([file.Blob], file.FileName),
                S3URL,
                fetchImpl
            )
        );
    }
</script>

<ComposedModal size="lg" bind:open on:submit>
    <ModalHeader title={`${file.FileName} kopieren`} />
    <ModalBody>
        <Grid fullWidth condensed>
            <Row>
                <Column>
                    <RadioButtonGroup
                        bind:selected={selectedTable}
                        orientation="vertical"
                        on:change={radio_change}
                    >
                        {#each tables as radio}
                            <RadioButton
                                labelText={radio.value}
                                value={radio.key}
                            />
                        {/each}
                    </RadioButtonGroup>
                </Column>
                <Column>
                    <DataTable
                        zebra
                        style="max-height: 1em"
                        radio
                        bind:selectedRowIds
                        {headers}
                        {rows}
                    />
                </Column>
            </Row>
        </Grid>
    </ModalBody>
    <ModalFooter>
        <Button kind="secondary" on:click={close}>Abbrechen</Button>
        <Button kind="primary" on:click={copy}>Kopieren</Button>
    </ModalFooter>
</ComposedModal>
