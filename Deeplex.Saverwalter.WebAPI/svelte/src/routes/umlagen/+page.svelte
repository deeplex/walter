<script lang="ts">
	import { goto } from '$app/navigation';
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../components/WalterDataTable.svelte';
	import { request_options } from '../../services/utils';
	import type { UmlageListEntry } from '../../types/umlagelist.type';

	const headers = [
		{ key: 'typ', value: 'Typ' },
		{ key: 'wohnungen', value: 'Wohnungen' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/umlagen/${e.detail.id}`);

	const async_rows: Promise<UmlageListEntry> = fetch(
		'/api/umlagen',
		request_options
	).then((e) => e.json());
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
