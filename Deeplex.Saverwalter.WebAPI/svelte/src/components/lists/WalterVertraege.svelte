<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper, WalterVertrag } from '$WalterComponents';
	import type { WalterSelectionEntry, WalterVertragEntry } from '$WalterTypes';

	const headers = [
		{ key: 'wohnung.text', value: 'Wohnung' },
		{ key: 'mieterAuflistung', value: 'Mieter' },
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' }
	];

	const addUrl = `/api/vertraege/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/vertraege/${e.detail.id}`);

	export let rows: WalterVertragEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;
	export let kontakte: WalterSelectionEntry[];
	export let wohnungen: WalterSelectionEntry[];
	export let a: Partial<WalterVertragEntry> | undefined = undefined;
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
		<WalterVertrag {kontakte} {wohnungen} {a} />
	{/if}
</WalterDataWrapper>
