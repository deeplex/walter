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

	export let rows: WalterUmlageEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;
	export let a: Partial<WalterUmlageEntry> | undefined = undefined;
</script>

<WalterDataWrapper
	addEntry={a}
	{addUrl}
	{title}
	{search}
	{navigate}
	{rows}
	{headers}
>
	{#if a}
		<WalterUmlage {a} />
	{/if}
</WalterDataWrapper>
