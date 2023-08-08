<script lang="ts">
    import {
    Button,
        Content,
        DataTable,
        DataTableSkeleton,
        SkeletonPlaceholder,
        Toolbar,
        ToolbarContent,
        ToolbarSearch
    } from 'carbon-components-svelte';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        convertEuro,
        convertPercent,
        convertTime
    } from '$walter/services/utils';
    import { dates, formatToTableDate, time } from './WalterDataTable';
    import { Add } from 'carbon-icons-svelte';

    export let fullHeight = false;
    export let headers: {
        key: string;
        value: string;
    }[];
    export let rows: any[];
    export let add: (() => void) | undefined = undefined;

    export let navigate: (
        e: CustomEvent<DataTableRow>
    ) => Promise<void> | void = () => {};

    function shouldFilterRows(row: any, value: any) {
        const filteredValues = headers.map(headerObj => {
            const keys = headerObj.key.split('.');
            let value = row;
            keys.forEach(key => {
                if (value?.hasOwnProperty(key)) {
                    value = value[key];
                } else {
                    value = null;
                }
            });
            return value;
        });

        const values = `${value}`.toLowerCase().split(";").map(e => e.trim());
        return values.every(val => filteredValues.some(e => `${e}`.toLowerCase().includes(val)));
    }
</script>

<Content>
    {#await rows}
        <SkeletonPlaceholder style="margin:0; width: 100%; height:3rem" />
        <DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
    {:then x}
        <DataTable
            on:click:row={navigate}
            sortable
            zebra
            stickyHeader
            {headers}
            rows={x}
            class={fullHeight ? 'proper-list' : ''}
            style="cursor: pointer; max-height: none !important"
        >
            <Toolbar>
                <ToolbarContent>
                    <ToolbarSearch
                        placeholder="Suche mit ; separierter Liste..."
                        persistent
                        {shouldFilterRows}
                    />
                    {#if !!add}
                        <Button
                            on:click={add}
                            iconDescription="Eintrag hinzufügen"
                            icon={Add}>Eintrag hinzufügen</Button
                            >
                    {/if}
                </ToolbarContent>
            </Toolbar>
            <span
                style="text-overflow: ellipsis; white-space: nowrap; overflow:hidden;"
                slot="cell"
                let:cell
            >
                {#if cell.value === null || cell.value === undefined || cell.value === ''}
                    ---
                {:else if dates(cell.key)}
                    {formatToTableDate(cell.value)}
                {:else if time(cell.key)}
                    {convertTime(cell.value)}
                {:else if
                    cell.key === 'betrag' ||
                    cell.key === 'grundmiete' ||
                    cell.key === 'kosten' ||
                    cell.key === 'gesamtBetrag'}
                    {convertEuro(cell.value)}
                {:else if cell.key === 'anteil'}
                    {convertPercent(cell.value)}
                {:else}
                    {cell.value}
                {/if}
            </span>
        </DataTable>

        <style>
            .proper-list
                > .bx--data-table_inner-container
                > .bx--data-table--sticky-header {
                max-height: none !important;
            }
        </style>
    {/await}
</Content>
