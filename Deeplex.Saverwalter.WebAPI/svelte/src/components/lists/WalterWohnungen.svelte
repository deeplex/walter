<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper, WalterWohnung } from '$WalterComponents';
	import type { WalterSelectionEntry, WalterWohnungEntry } from '$WalterTypes';

	const headers = [
		{ key: 'adresse.anschrift', value: 'Anschrift' },
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'besitzer.text', value: 'Besitzer' },
		{ key: 'bewohner', value: 'Bewohner' }
	];

	const addUrl = `/api/wohnungen/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/wohnungen/${e.detail.id}`);

	export let rows: WalterWohnungEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;
	export let kontakte: WalterSelectionEntry[];
	export let a: Partial<WalterWohnungEntry> | undefined = undefined;
</script>

<WalterDataWrapper
	{addUrl}
	addEntry={a}
	{title}
	{search}
	{navigate}
	{rows}
	{headers}
>
	{#if a}
		<WalterWohnung {kontakte} {a} />
	{/if}
</WalterDataWrapper>
