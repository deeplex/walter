<script lang="ts">
	import { goto } from '$app/navigation';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	import { WalterDataWrapper, WalterWohnung } from '$WalterComponents';
	import type { WalterAdresseEntry, WalterWohnungEntry } from '$WalterTypes';
	import WalterAdresse from '../subdetails/WalterAdresse.svelte';

	const headers = [
		{ key: 'strasse', value: 'Straße' },
		{ key: 'hausnummer', value: 'Hausnummer' },
		{ key: 'postleitzahl', value: 'Postleitzahl' },
		{ key: 'stadt', value: 'Stadt' }
	];

	const addUrl = `/api/wohnungen/`;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/adressen/${e.detail.id}`);

	export let rows: WalterAdresseEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;
	export let a: Partial<WalterAdresseEntry> | undefined = undefined;
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
		<WalterAdresse value={a} />
	{/if}
</WalterDataWrapper>
