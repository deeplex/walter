<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../components/WalterDataTable.svelte';
	import { walter_get } from '../../services/utils';
	import type { WohnungListEntry } from '../../types/wohnunglist.type';

	const headers = [
		{ key: 'kennnummer', value: 'Kennnummer' },
		{ key: 'wohnung', value: 'Wohnung' },
		{ key: 'typ', value: 'Typ' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/zaehler/${e.detail.id}`);

	const async_rows: Promise<WohnungListEntry[]> = walter_get('/api/zaehler');
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
