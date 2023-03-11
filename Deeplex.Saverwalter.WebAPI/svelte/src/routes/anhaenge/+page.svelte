<script lang="ts">
	import { goto } from '$app/navigation';
	import { WalterDataTable, WalterHeaderList } from '$WalterComponents';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import type { PageData } from './$types';

	export let data: PageData;

	const headers = [
		{ key: 'fileName', value: 'Dateiname' },
		{ key: 'creationTime', value: 'Erstellungszeitpunkt' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/anhaenge/${e.detail.id}`);
</script>

<WalterHeaderList nonav apiUrl={data.apiURL} title="Anhänge" />
<WalterDataTable {navigate} search {headers} rows={data.rows} />
