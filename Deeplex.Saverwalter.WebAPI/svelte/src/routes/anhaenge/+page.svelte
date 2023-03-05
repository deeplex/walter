<script lang="ts">
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { goto } from '$app/navigation';
	import { WalterDataTable, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;

	const headers = [
		{ key: 'fileName', value: 'Dateiname' },
		{ key: 'creationTime', value: 'Erstellungszeitpunkt' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/${data.url}/${e.detail.id}`);
</script>

<WalterHeaderList url={data.url} title="Anhänge" />

<WalterDataTable search {navigate} {headers} rows={data.rows} />
