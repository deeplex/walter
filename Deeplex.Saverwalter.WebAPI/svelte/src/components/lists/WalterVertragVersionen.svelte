<script lang="ts">
	import { goto } from '$app/navigation';
	import { WalterDataWrapper, WalterVertragVersion } from '$WalterComponents';
	import type { WalterVertragVersionEntry } from '$WalterTypes';
	import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

	const headers = [
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'personenzahl', value: 'Personenzahl' },
		{ key: 'grundmiete', value: 'Grundmiete' },
		{ key: 'notiz', value: 'Notiz' }
	];

	const addUrl = `/api/vertragversionen/`;

	export let rows: WalterVertragVersionEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	const navigate = (e: CustomEvent<DataTableRow>) =>
		goto(`/vertragversionen/${e.detail.id}`);

	export let a: Partial<WalterVertragVersionEntry> | undefined = undefined;
</script>

<WalterDataWrapper
	{navigate}
	{addUrl}
	addEntry={a}
	{title}
	{search}
	{rows}
	{headers}
>
	{#if a}
		<WalterVertragVersion {a} />
	{/if}
</WalterDataWrapper>
