<script lang="ts">
	import { goto } from '$app/navigation';
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import type { KontaktListEntry } from '../../types/kontaktlistentry.type';

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
		{ key: 'mobil', value: 'Mobil' },
		{ key: 'email', value: 'E-Mail' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/kontakte/${e.detail.id > 0 ? 'nat' : 'jur'}/${Math.abs(e.detail.id)}`);

	const async_rows: Promise<KontaktListEntry[]> = fetch('/api/kontakte', request_options).then(
		(e) => e.json()
	);
</script>

{#await async_rows}
	<DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
{:then rows}
	<DataTable on:click:row={navigate} sortable zebra stickyHeader {headers} {rows} />
{/await}
