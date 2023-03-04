<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper, WalterUmlage } from '$WalterComponents';
	import type { WalterUmlageEntry } from '$WalterTypes';

	const headers = [
		{ key: 'typ.text', value: 'Typ' },
		{ key: 'wohnungenBezeichnung', value: 'Wohnungen' }
	];

	const addUrl = `/api/umlagen/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/umlagen/${e.detail.id}`);

	export let rows: Promise<WalterUmlageEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let entry: Partial<WalterUmlageEntry> | undefined = undefined;
</script>

<WalterDataWrapper
	addEntry={entry}
	{addUrl}
	{title}
	{search}
	{navigate}
	{rows}
	{headers}
>
	{#if entry}
		<WalterUmlage {entry} />
	{/if}
</WalterDataWrapper>
