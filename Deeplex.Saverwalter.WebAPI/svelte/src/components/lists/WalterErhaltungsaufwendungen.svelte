<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import {
		WalterDataWrapper,
		WalterErhaltungsaufwendung
	} from '$WalterComponents';
	import type {
		WalterErhaltungsaufwendungEntry,
		WalterSelectionEntry
	} from '$WalterTypes';

	const headers = [
		{ key: 'bezeichnung', value: 'Bezeichnung' },
		{ key: 'aussteller.text', value: 'Aussteller' },
		{ key: 'wohnung.text', value: 'Wohnung' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const addUrl = `/api/erhaltungsaufwendungen/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/erhaltungsaufwendungen/${e.detail.id}`);

	export let rows: WalterErhaltungsaufwendungEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;
	export let kontakte: WalterSelectionEntry[];
	export let wohnungen: WalterSelectionEntry[];

	export let a: Partial<WalterErhaltungsaufwendungEntry> | undefined =
		undefined;
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
		<WalterErhaltungsaufwendung {kontakte} {wohnungen} {a} />
	{/if}
</WalterDataWrapper>
