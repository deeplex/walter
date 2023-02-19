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

	export let headers: {
		key: string;
		value: string;
	}[];
	export let rows: Promise<any[]>;

	export let navigate: (
		e: CustomEvent<DataTableRow>
	) => Promise<void> | void = () => {};
	export let search: boolean = false;
</script>

<Content>
	{#await rows}
		{#if search}
			<SkeletonPlaceholder style="margin:0; width: 100%; height:3rem" />
		{/if}
		<DataTableSkeleton
			style="min-width: 50rem;"
			{headers}
			showHeader={false}
			showToolbar={false}
		/>
	{:then rows}
		<DataTable
			on:click:row={navigate}
			sortable
			zebra
			stickyHeader
			{headers}
			{rows}
		>
			{#if search}
				<Toolbar>
					<ToolbarContent>
						<ToolbarSearch placeholder="Suche..." persistent shouldFilterRows />
					</ToolbarContent>
				</Toolbar>
			{/if}
		</DataTable>
	{/await}
</Content>
