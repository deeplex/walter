<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
	import { WalterDataWrapper } from '..';
	import type { ErhaltungsaufwendungEntry } from '../../types/erhaltungsaufwendung.type';

	const headers = [
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'aussteller.name', value: 'Aussteller' },
		{ key: 'wohnung.anschrift', value: 'Wohnung' },
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
