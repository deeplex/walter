<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper, WalterZaehler } from '$WalterComponents';
	import type { WalterSelectionEntry, WalterZaehlerEntry } from '$WalterTypes';

	const headers = [
		{ key: 'kennnummer', value: 'Kennnummer' },
		{ key: 'wohnung.text', value: 'Wohnung' },
		{ key: 'typ.text', value: 'Typ' }
	];

	const addUrl = `/api/zaehler/`;

	export let rows: WalterZaehlerEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;
	export let wohnungen: WalterSelectionEntry[];
	export let zaehler: WalterSelectionEntry[];
	export let zaehlertypen: WalterSelectionEntry[];
	export let a: Partial<WalterZaehlerEntry> | undefined = undefined;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/zaehler/${e.detail.id}`);
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
		<WalterZaehler {wohnungen} {zaehler} {zaehlertypen} {a} />
	{/if}
</WalterDataWrapper>
