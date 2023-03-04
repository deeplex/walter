<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper, WalterWohnung } from '$WalterComponents';
	import type { WalterWohnungEntry } from '$WalterTypes';

	const headers = [
		{ key: 'adresse.anschrift', value: 'Anschrift' },
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'besitzer.text', value: 'Besitzer' },
		{ key: 'bewohner', value: 'Bewohner' }
	];

	const addUrl = `/api/wohnungen/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/wohnungen/${e.detail.id}`);

	export let rows: Promise<WalterWohnungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let entry: Partial<WalterWohnungEntry> | undefined = undefined;
</script>

<WalterDataWrapper
	{addUrl}
	addEntry={entry}
	{title}
	{search}
	{navigate}
	{rows}
	{headers}
>
	{#if entry}
		<WalterWohnung {entry} />
	{/if}
</WalterDataWrapper>
