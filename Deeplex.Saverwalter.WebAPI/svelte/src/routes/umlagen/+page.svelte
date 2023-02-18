<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../components/WalterDataTable.svelte';
	import { walter_get } from '../../services/utils';
	import type { UmlageListEntry } from '../../types/umlagelist.type';

	const headers = [
		{ key: 'typ', value: 'Typ' },
		{ key: 'wohnungen', value: 'Wohnungen' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/umlagen/${e.detail.id}`);

	const async_rows: Promise<UmlageListEntry> = walter_get('/api/umlagen');
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
