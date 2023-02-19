<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { WalterHeader } from '../../components';
	import Betriebskostenrechnungen from '../../components/lists/Betriebskostenrechnungen.svelte';
	import { walter_get } from '../../services/utils';
	import type { BetriebskostenrechnungListEntry } from '../../types/betriebskostenrechnunglist.type';

	const headers = [
		{ key: 'typ', value: 'Typ' },
		{ key: 'wohnungenBezeichnung', value: 'Wohnungen' },
		{ key: 'betreffendesJahr', value: 'Betreffendes Jahr' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const rows: Promise<BetriebskostenrechnungListEntry[]> = walter_get(
		'/api/betriebskostenrechnungen'
	);
</script>

<WalterHeader title="Betriebskostenrechnung" />
<Betriebskostenrechnungen search {rows} />
