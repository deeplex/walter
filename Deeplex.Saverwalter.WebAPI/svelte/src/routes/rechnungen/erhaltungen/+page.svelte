<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../../components/WalterDataTable.svelte';
	import { walter_get } from '../../../services/utils';
	import type { ErhaltungsaufwendungListEntry } from '../../../types/erhaltungsaufwendunglist.type';

	const headers = [
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'aussteller', value: 'Aussteller' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/rechnungen/erhaltungen/${e.detail.id}`);

	const async_rows: Promise<ErhaltungsaufwendungListEntry[]> = walter_get(
		'/api/erhaltungsaufwendungen'
	);
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
