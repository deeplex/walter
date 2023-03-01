<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper } from '$components';
	import type { WohnungEntry } from '$types';

	const headers = [
		{ key: 'adresse.anschrift', value: 'Anschrift' },
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'besitzer.text', value: 'Besitzer' },
		{ key: 'bewohner', value: 'Bewohner' }
	];
	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/wohnungen/${e.detail.id}`);

	export let rows: Promise<WohnungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;
</script>

<WalterDataWrapper {title} {search} {navigate} {rows} {headers} />
