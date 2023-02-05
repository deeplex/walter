<script lang="ts">
	import { goto } from '$app/navigation';
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const headers = [
		{ key: 'name', value: 'Name' },
		{ key: 'anschrift', value: 'Anschrift' },
		{ key: 'telefon', value: 'Telefon' },
		{ key: 'email', value: 'E-Mail' }
	];

	class KontaktListEntry {
		id: string;
		guid: string;
		name: string;
		anschrift: string;
		email: string;
		telefon: string;

		constructor(e: KontaktListEntry) {
			this.guid = e.guid;
			this.id = this.guid;
			this.name = e.name;
			this.anschrift = e.anschrift;
			this.email = e.email;
			this.telefon = e.telefon;
		}
	}

	const navigate = (e: CustomEvent<DataTableRow>) => goto(`/kontakte/${e.detail.guid}`);

	const async_rows = fetch('/api/kontakte', request_options)
		.then((e) => e.json())
		.then((j) => j.map((k: KontaktListEntry) => new KontaktListEntry(k)));
</script>

<h1>
	{#await async_rows}
		<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
	{:then rows}
		<DataTable on:click:row={navigate} sortable zebra stickyHeader {headers} {rows} />
	{/await}
</h1>
