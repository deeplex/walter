<script lang="ts">
    import {
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

    export let headers: {
        key: string;
        value: string;
    }[];
    export let rows: any[];

    export let navigate: (
        e: CustomEvent<DataTableRow>
    ) => Promise<void> | void = () => {};
    export let search = false;

    // TODO make that smarter...
    function dates(key: string) {
        switch (key) {
            case 'beginn':
            case 'ende':
            case 'datum':
            case 'betreffenderMonat':
            case 'zahlungsdatum':
                return true;
            default:
                return false;
        }
    }

    function time(key: string) {
        switch (key) {
            case 'creationTime':
            case 'LastModified':
                return true;
            default:
                return false;
        }
    }

    function formatToTableDate(date: string) {
        return new Date(date).toLocaleDateString('de-DE', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    }
</script>

<Content>
    {#await rows}
        {#if search}
            <SkeletonPlaceholder style="margin:0; width: 100%; height:3rem" />
        {/if}
        <DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
    {:then x}
        <DataTable
            on:click:row={navigate}
            sortable
            zebra
            stickyHeader
            {headers}
            rows={x}
            style="cursor: pointer"
        >
            {#if search}
                <Toolbar>
                    <ToolbarContent>
                        <ToolbarSearch
                            placeholder="Suche..."
                            persistent
                            shouldFilterRows
                        />
                    </ToolbarContent>
                </Toolbar>
            {/if}
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
                {:else if cell.key === 'betrag' || cell.key === 'grundmiete' || cell.key === 'kosten'}
                    {convertEuro(cell.value)}
                {:else if cell.key === 'anteil'}
                    {convertPercent(cell.value)}
                {:else}
                    {cell.value}
                {/if}
            </span>
        </DataTable>
    {/await}
</Content>
