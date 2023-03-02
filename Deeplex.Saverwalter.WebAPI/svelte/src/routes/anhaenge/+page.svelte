<script lang="ts">
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { goto } from '$app/navigation';

	import { walter_get } from '$services/requests';
	import { WalterDataTable, WalterHeaderList } from '$components';
	import type { AnhangEntry } from '$types';

	const headers = [
		{ key: 'fileName', value: 'Dateiname' },
		{ key: 'creationTime', value: 'Erstellungszeitpunkt' }
	];

	const url = 'anhaenge';

	const rows: Promise<AnhangEntry[]> = walter_get(`/api/${url}`);

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/${url}/${e.detail.id}`);
</script>

<WalterHeaderList {url} title="Anhänge" />

<WalterDataTable search {navigate} {headers} {rows} />
