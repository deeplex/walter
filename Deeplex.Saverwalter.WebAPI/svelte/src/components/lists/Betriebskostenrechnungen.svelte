<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper } from '$components';
	import type { BetriebskostenrechnungEntry } from '$types';

	const headers = [
		{ key: 'typ', value: 'Typ' },
		{ key: 'wohnungenBezeichnung', value: 'Wohnungen' },
		{ key: 'betreffendesJahr', value: 'Betreffendes Jahr' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/betriebskostenrechnungen/${e.detail.id}`);

	export let rows: Promise<BetriebskostenrechnungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
