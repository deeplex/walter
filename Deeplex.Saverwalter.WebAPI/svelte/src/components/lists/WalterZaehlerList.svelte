<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper, WalterZaehler } from '$WalterComponents';
	import type { WalterZaehlerEntry } from '$WalterTypes';

	const headers = [
		{ key: 'kennnummer', value: 'Kennnummer' },
		{ key: 'wohnung.text', value: 'Wohnung' },
		{ key: 'typ.text', value: 'Typ' }
	];

	const addUrl = `/api/zaehler/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/zaehler/${e.detail.id}`);

	export let rows: Promise<WalterZaehlerEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let a: Promise<Partial<WalterZaehlerEntry>> | undefined = undefined;
	let entry: Partial<WalterZaehlerEntry> | undefined = undefined;
	a?.then((e) => (entry = e));
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
		<WalterZaehler {a} {entry} />
	{/if}
</WalterDataWrapper>
