<script lang="ts">
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';

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

	class ErhaltungsaufwendungListEntry {
		id: number;
		bezeichnung: string;
		aussteller: string;
		betrag: string;
		datum: string;

		constructor(e: ErhaltungsaufwendungListEntry) {
			this.id = e.id;
			this.bezeichnung = e.bezeichnung;
			this.aussteller = e.aussteller;
			this.betrag = e.betrag;
			this.datum = e.datum;
		}
	}

	const async_rows = fetch('/api/erhaltungsaufwendungen', request_options)
		.then((e) => e.json())
		.then((j) => j.map((v: ErhaltungsaufwendungListEntry) => new ErhaltungsaufwendungListEntry(v)));
</script>

<h1>
	{#await async_rows}
		<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
	{:then rows}
		<DataTable sortable zebra stickyHeader {headers} {rows} />
	{/await}
</h1>
