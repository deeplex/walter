<script lang="ts">
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const headers = [
		{ key: 'typ', value: 'Typ' },
		{ key: 'wohnungen', value: 'Wohnungen' },
		{ key: 'betreffendesjahr', value: 'Betreffendes Jahr' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	class BetriebskostenrechnungListEntry {
		id: number;
		typ: string;
		wohnungen: string;
		betrag: string;
		betreffendesjahr: string;
		datum: string;

		constructor(e: BetriebskostenrechnungListEntry) {
			this.id = e.id;
			this.typ = e.typ;
			this.betrag = e.betrag;
			this.wohnungen = e.wohnungen;
			this.betreffendesjahr = e.betreffendesjahr;
			this.datum = e.datum;
		}
	}

	const async_rows = fetch('/api/betriebskostenrechnungen', request_options)
		.then((e) => e.json())
		.then((j) =>
			j.map((v: BetriebskostenrechnungListEntry) => new BetriebskostenrechnungListEntry(v))
		);
</script>

{#await async_rows}
	<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
{:then rows}
	<DataTable sortable zebra stickyHeader {headers} {rows} />
{/await}
