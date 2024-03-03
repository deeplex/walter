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
