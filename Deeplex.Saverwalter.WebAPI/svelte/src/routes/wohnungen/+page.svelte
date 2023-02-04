<script lang="ts">
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const headers = [
		{ key: 'anschrift', value: 'Anschrift' },
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'besitzer', value: 'Besitzer' },
		{ key: 'bewohner', value: 'Bewohner' }
	];

	class WohnungListEntry {
		id: number;
		anschrift: string;
		bezeichnung: string;
		besitzer: string;
		bewohner: string;

		constructor(e: WohnungListEntry) {
			this.id = e.id;
			this.bezeichnung = e.bezeichnung;
			this.anschrift = e.anschrift;
			this.besitzer = e.besitzer;
			this.bewohner = e.bewohner;
		}
	}

	const async_rows = fetch('/api/wohnungen', request_options)
		.then((e) => e.json())
		.then((j) => j.map((w: WohnungListEntry) => new WohnungListEntry(w)));
</script>

<h1>
	{#await async_rows}
		<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
	{:then rows}
		<DataTable sortable zebra stickyHeader {headers} {rows} />
	{/await}
</h1>
