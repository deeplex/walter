<script lang="ts">
	import {
		DataTable,
		DataTableSkeleton,
		Toolbar,
		ToolbarContent,
		ToolbarSearch
	} from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	export let headers: {
		key: string;
		value: string;
	}[];
	export let async_rows: Promise<any>;

	export let navigate: (e: CustomEvent<DataTableRow>) => Promise<void>;
</script>

{#await async_rows}
	<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
{:then rows}
	<DataTable
		on:click:row={navigate}
		sortable
		zebra
		stickyHeader
		{headers}
		{rows}
	>
		<Toolbar>
			<ToolbarContent>
				<ToolbarSearch persistent shouldFilterRows />
			</ToolbarContent>
		</Toolbar>
	</DataTable>
{/await}
