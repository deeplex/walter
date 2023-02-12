<script lang="ts">
	import { goto } from '$app/navigation';
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../components/WalterDataTable.svelte';
	import { request_options } from '../../services/utilts';

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

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/wohnungen/${e.detail.id}`);

	const async_rows = fetch('/api/wohnungen', request_options)
		.then((e) => e.json())
		.then((j) => j.map((w: WohnungListEntry) => new WohnungListEntry(w)));
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
