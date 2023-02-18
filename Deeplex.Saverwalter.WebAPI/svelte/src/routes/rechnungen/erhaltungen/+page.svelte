<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../../components/WalterDataTable.svelte';
	import WalterHeader from '../../../components/WalterHeader.svelte';
	import { walter_get } from '../../../services/utils';
	import type { ErhaltungsaufwendungListEntry } from '../../../types/erhaltungsaufwendunglist.type';

	const headers = [
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'aussteller', value: 'Aussteller' },
		{ key: 'wohnung', value: 'Wohnung' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/rechnungen/erhaltungen/${e.detail.id}`);

	const rows: Promise<ErhaltungsaufwendungListEntry[]> = walter_get(
		'/api/erhaltungsaufwendungen'
	);
</script>

<WalterHeader title="Erhaltungsaufwendungen" />

<WalterDataTable {navigate} {rows} {headers} />
