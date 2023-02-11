<script lang="ts">
	import { goto } from '$app/navigation';
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { request_options } from '../../../services/utilts';
	import type { ErhaltungsaufwendungListEntry } from '../../../types/erhaltungsaufwendunglist.type';

	const headers = [
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'aussteller', value: 'Aussteller' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/rechnungen/erhaltungen/${e.detail.id}`);

	const async_rows: Promise<ErhaltungsaufwendungListEntry[]> = fetch(
		'/api/erhaltungsaufwendungen',
		request_options
	).then((e) => e.json());
</script>

<h1>
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
		/>
	{/await}
</h1>
