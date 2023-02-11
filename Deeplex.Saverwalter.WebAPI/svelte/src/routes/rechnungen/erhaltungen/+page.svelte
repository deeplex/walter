<script lang="ts">
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { ErhaltungsaufwendungListEntry } from '../../../types/erhaltungsaufwendunglist.type';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const headers = [
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'aussteller', value: 'Aussteller' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const async_rows: Promise<ErhaltungsaufwendungListEntry[]> = fetch(
		'/api/erhaltungsaufwendungen',
		request_options
	).then((e) => e.json());
</script>

<h1>
	{#await async_rows}
		<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
	{:then rows}
		<DataTable sortable zebra stickyHeader {headers} {rows} />
	{/await}
</h1>
