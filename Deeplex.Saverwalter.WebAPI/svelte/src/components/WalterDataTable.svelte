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
	import { convertDate, convertEuro, convertTime } from '../services/utils';

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
	{:then x}
		<DataTable
			on:click:row={navigate}
			sortable
			zebra
			stickyHeader
			{headers}
			rows={x}
		>
			{#if search}
				<Toolbar>
					<ToolbarContent>
						<ToolbarSearch placeholder="Suche..." persistent shouldFilterRows />
					</ToolbarContent>
				</Toolbar>
			{/if}
			<!-- style="margin-top: -0.5em; line-height: 1rem; text-align: center; width: 100%" -->
			<span slot="cell" let:cell>
				{#if cell.value === null || cell.value === undefined || cell.value === ''}
					---
				{:else if cell.key === 'beginn' || cell.key === 'ende' || cell.key === 'datum'}
					{convertDate(cell.value)}
				{:else if cell.key === 'creationTime'}
					{convertTime(cell.value)}
				{:else if cell.key === 'betrag'}
					{convertEuro(cell.value)}
				{:else}
					{cell.value}
				{/if}
			</span>
		</DataTable>
	{/await}
</Content>
