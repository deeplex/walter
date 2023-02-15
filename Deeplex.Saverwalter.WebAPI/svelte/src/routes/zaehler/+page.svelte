<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../components/WalterDataTable.svelte';
	import { request_options } from '../../services/utils';
	import type { WohnungListEntry } from '../../types/wohnunglist.type';

	const headers = [
		{ key: 'kennnummer', value: 'Kennnummer' },
		{ key: 'wohnung', value: 'Wohnung' },
		{ key: 'typ', value: 'Typ' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/zaehler/${e.detail.id}`);

	const async_rows: Promise<WohnungListEntry[]> = fetch(
		'/api/zaehler',
		request_options
	).then((e) => e.json());
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
