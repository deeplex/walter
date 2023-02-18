<script lang="ts">
	import { goto } from '$app/navigation';
	import { DataTable, DataTableSkeleton } from 'carbon-components-svelte';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import WalterDataTable from '../../../components/WalterDataTable.svelte';
	import { walter_get } from '../../../services/utils';
	import type { BetriebskostenrechnungListEntry } from '../../../types/betriebskostenrechnunglist.type';

	const headers = [
		{ key: 'typ', value: 'Typ' },
		{ key: 'wohnungenBezeichnung', value: 'Wohnungen' },
		{ key: 'betreffendesJahr', value: 'Betreffendes Jahr' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/rechnungen/betriebskosten/${e.detail.id}`);

	const async_rows: Promise<BetriebskostenrechnungListEntry[]> = walter_get(
		'/api/betriebskostenrechnungen'
	);
</script>

<WalterDataTable {navigate} {async_rows} {headers} />
