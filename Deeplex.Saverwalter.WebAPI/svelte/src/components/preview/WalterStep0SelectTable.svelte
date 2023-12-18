<script lang="ts">
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';
    import { WalterDataTable } from '..';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    export let step: number;
    export let tables: WalterPreviewCopyTable[];
    export let selectedTable_change: (key: string) => Promise<void>;

    const headers = [{ key: 'text', value: 'Tabelle' }];
    const rows = tables.map((table) => ({
        id: table.key,
        text: table.value
    }));

    function clicked(row: CustomEvent<DataTableRow>) {
        selectedTable_change(row.detail.id);
    }
</script>

{#if step === 0}
    <WalterDataTable on_click_row={clicked} fullHeight {headers} {rows} />
{/if}
