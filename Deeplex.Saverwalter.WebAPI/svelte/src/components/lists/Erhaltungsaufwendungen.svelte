<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper } from '$components';
	import type { ErhaltungsaufwendungEntry } from '$types';

	const headers = [
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'aussteller.text', value: 'Aussteller' },
		{ key: 'wohnung.text', value: 'Wohnung' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/erhaltungsaufwendungen/${e.detail.id}`);

	export let rows: Promise<ErhaltungsaufwendungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
