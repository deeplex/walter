<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import {
		WalterBetriebskostenrechnung,
		WalterDataWrapper
	} from '$WalterComponents';
	import type { WalterBetriebskostenrechnungEntry } from '$WalterTypes';

	export let rows: WalterBetriebskostenrechnungEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	const headers = [
		{ key: 'typ.text', value: 'Typ' },
		{ key: 'umlage.text', value: 'Wohnungen' },
		{ key: 'betreffendesJahr', value: 'Betreffendes Jahr' },
		{ key: 'betrag', value: 'Betrag' },
		{ key: 'datum', value: 'Datum' }
	];

	const addUrl = `/api/betriebskostenrechnungen/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/betriebskostenrechnungen/${e.detail.id}`);

	export let a: Partial<WalterBetriebskostenrechnungEntry> | undefined =
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
		<WalterBetriebskostenrechnung {a} />
	{/if}
</WalterDataWrapper>
