<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper } from '$WalterComponents';
	import type { WalterVertragEntry } from '$WalterTypes';
	import WalterVertrag from '../details/WalterVertrag.svelte';

	const headers = [
		{ key: 'wohnung.text', value: 'Wohnung' },
		{ key: 'mieterAuflistung', value: 'Mieter' },
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' }
	];

	const addUrl = `/api/vertraege/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/vertraege/${e.detail.id}`);

	export let rows: Promise<WalterVertragEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let a: Promise<Partial<WalterVertragEntry>> | undefined = undefined;
	let entry: Partial<WalterVertragEntry> | undefined = undefined;
	a?.then((e) => (entry = e));
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
		<WalterVertrag {a} {entry} />
	{/if}
</WalterDataWrapper>
