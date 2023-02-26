<script lang="ts">
	import { walter_get } from '../../services/utils';
	import { WalterHeader, WalterDataTable } from '../../components';
	import type { AnhangEntry } from '../../types/anhang.type';
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	const headers = [
		{ key: 'fileName', value: 'Dateiname' },
		{ key: 'creationTime', value: 'Erstellungszeitpunkt' }
	];

	const rows: Promise<AnhangEntry[]> = walter_get('/api/anhaenge');

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/anhaenge/${e.detail.id}`);
</script>

<WalterHeader title="Anhänge" />

<WalterDataTable search {navigate} {headers} {rows} />
