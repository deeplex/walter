<script lang="ts">
	import { goto } from '$app/navigation';
	import { WalterDataWrapper, WalterMiete } from '$WalterComponents';
	import type { WalterMieteEntry } from '$WalterTypes';

	const headers = [
		{ key: 'betreffenderMonat', value: 'Betreffender Monat' },
		{ key: 'zahlungsdatum', value: 'Zahlungsdatum' },
		{ key: 'betrag', value: 'Betrag' }
	];

	const addUrl = `/api/mieten/`;

	export let rows: WalterMieteEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	const navigate = (e: CustomEvent) => goto(`/mieten/${e.detail.id}`);

	export let a: Partial<WalterMieteEntry> | undefined = undefined;
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
		<WalterMiete {a} />
	{/if}
</WalterDataWrapper>
