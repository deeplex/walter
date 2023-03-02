<script lang="ts">
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { goto } from '$app/navigation';

	import { walter_get } from '$WalterServices/requests';
	import { WalterDataTable, WalterHeaderList } from '$WalterComponents';
	import type { WalterAnhangEntry } from '$WalterTypes';

	const headers = [
		{ key: 'fileName', value: 'Dateiname' },
		{ key: 'creationTime', value: 'Erstellungszeitpunkt' }
	];

	const url = 'anhaenge';

	const rows: Promise<WalterAnhangEntry[]> = walter_get(`/api/${url}`);

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/${url}/${e.detail.id}`);
</script>

<WalterHeaderList {url} title="Anhänge" />

<WalterDataTable search {navigate} {headers} {rows} />
