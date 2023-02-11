<script lang="ts">
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { BetriebskostenrechnungListEntry } from '../../../types/betriebskostenrechnunglist.type';

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

	const async_rows: Promise<BetriebskostenrechnungListEntry[]> = fetch(
		'/api/betriebskostenrechnungen',
		request_options
	).then((e) => e.json());
</script>

{#await async_rows}
	<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
{:then rows}
	<DataTable sortable zebra stickyHeader {headers} {rows} />
{/await}
