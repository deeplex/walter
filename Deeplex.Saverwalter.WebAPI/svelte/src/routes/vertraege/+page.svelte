<script lang="ts">
	import { goto } from '$app/navigation';
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../components/WalterDataTable.svelte';
	import { request_options } from '../../services/utils';

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

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/vertraege/${e.detail.id}`);

	const async_rows = fetch('/api/vertraege', request_options)
		.then((e) => e.json())
		.then((j) => j.map((v: VertragListEntry) => new VertragListEntry(v)));
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
