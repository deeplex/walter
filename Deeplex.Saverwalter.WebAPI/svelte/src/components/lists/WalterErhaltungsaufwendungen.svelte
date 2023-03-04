<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import {
		WalterDataWrapper,
		WalterErhaltungsaufwendung
	} from '$WalterComponents';
	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';

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

	export let rows: Promise<WalterErhaltungsaufwendungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	export let entry: Partial<WalterErhaltungsaufwendungEntry> | undefined =
		undefined;
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
		<WalterErhaltungsaufwendung {entry} />
	{/if}
</WalterDataWrapper>
