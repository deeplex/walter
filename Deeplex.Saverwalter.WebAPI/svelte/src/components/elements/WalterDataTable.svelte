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

	import { convertDate, convertEuro, convertTime } from '$WalterServices/utils';

	export let headers: {
		key: string;
		value: string;
	}[];
	export let rows: any[];

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
						<ToolbarSearch placeholder="Suche..." persistent shouldFilterRows />
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
				{:else if cell.key === 'beginn' || cell.key === 'ende' || cell.key === 'datum' || cell.key === 'betreffenderMonat' || cell.key === 'zahlungsdatum'}
					{convertDate(cell.value)}
				{:else if cell.key === 'creationTime'}
					{convertTime(cell.value)}
				{:else if cell.key === 'betrag' || cell.key === 'grundmiete'}
					{convertEuro(cell.value)}
				{:else}
					{cell.value}
				{/if}
			</span>
		</DataTable>
	{/await}
</Content>
