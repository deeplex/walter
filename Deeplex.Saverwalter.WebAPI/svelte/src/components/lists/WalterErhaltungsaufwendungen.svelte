<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper } from '$WalterComponents';
	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';

	const headers = [
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'aussteller.text', value: 'Aussteller' },
		{ key: 'wohnung.text', value: 'Wohnung' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/erhaltungsaufwendungen/${e.detail.id}`);

	export let rows: Promise<WalterErhaltungsaufwendungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
