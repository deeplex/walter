<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../components/WalterDataTable.svelte';
	import { walter_get } from '../../services/utils';
	import type { WohnungListEntry } from '../../types/wohnunglist.type';

	const headers = [
		{ key: 'anschrift', value: 'Anschrift' },
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'besitzer', value: 'Besitzer' },
		{ key: 'bewohner', value: 'Bewohner' }
	];
	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/wohnungen/${e.detail.id}`);

	const async_rows: Promise<WohnungListEntry[]> = walter_get('/api/wohnungen');
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
