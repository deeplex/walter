<script lang="ts">
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const headers = [
		{ key: 'wohnung', value: 'Wohnung' },
		{ key: 'mieter', value: 'Mieter' },
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' }
	];

	class VertragListEntry {
		id: number;
		mieter: string;
		wohnung: string;
		beginn: string;
		ende: string;

		constructor(e: VertragListEntry) {
			this.id = e.id;
			this.mieter = e.mieter;
			this.wohnung = e.wohnung;
			this.beginn = e.beginn;
			this.ende = e.ende;
		}
	}

	const async_rows = fetch('/api/vertraege', request_options)
		.then((e) => e.json())
		.then((j) => j.map((v: VertragListEntry) => new VertragListEntry(v)));
</script>

<h1>
	{#await async_rows}
		<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
	{:then rows}
		<DataTable sortable zebra stickyHeader {headers} {rows} />
	{/await}
</h1>
