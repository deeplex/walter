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
    import { dates, euro, formatToTableDate, time } from './WalterDataTable';
    import { Add } from 'carbon-icons-svelte';

    export let fullHeight = false;
    export let size: 'compact' | 'short' | 'medium' | 'tall' | undefined =
        undefined;
    export let headers: {
        key: string;
        value: string;
    }[];
    export let rows: unknown[];
    export let add: (() => void) | undefined = undefined;

    export let on_click_row: (
        e: CustomEvent<DataTableRow>
    ) => Promise<void> | void = () => {};

    function shouldFilterRows(row: { [key: string]: unknown }, value: unknown) {
        const filteredValues = headers.map((headerObj) => {
            const keys = headerObj.key.split('.');
            let value: unknown | null = row;
            keys.forEach((key) => {
                if (
                    typeof value === 'object' &&
                    value !== null &&
                    key in value
                ) {
                    value = (value as { [key: string]: unknown })[key];
                } else {
                    value = null;
                }
            });
            return value;
        });

        const values = `${value}`
            .toLowerCase()
            .split(';')
            .map((e) => e.trim());
        return values.every((val) =>
            filteredValues.some((e) => `${e}`.toLowerCase().includes(val))
        );
    }
</script>

<Content>
    {#await rows}
        <SkeletonPlaceholder style="margin:0; width: 100%; height:3rem" />
        <DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
    {:then x}
        <DataTable
            {size}
            on:click:row={on_click_row}
            sortable
            zebra
            stickyHeader
            {headers}
            rows={x}
            class={fullHeight ? 'proper-list' : ''}
            style="cursor: pointer; max-height: none !important;"
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
                {:else if euro(cell.key)}
                    {convertEuro(cell.value)}
                {:else if cell.key === 'anteil'}
                    {convertPercent(cell.value)}
                {:else if cell.key === 'button'}
                    <Button
                        disabled={cell.value === 'disabled'}
                        on:click={cell.value}
                        tooltipPosition="left"
                        style="position: absolute; margin-top: -15px; margin-left: 1em; scale: 0.65"
                        kind="tertiary"
                        icon={Add}
                        iconDescription={'Hinzufügen'}
                    />
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
