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

	export let rows: Promise<WalterMieteEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	const navigate = (e: CustomEvent) => goto(`/mieten/${e.detail.id}`);

	export let a: Promise<Partial<WalterMieteEntry>> | undefined = undefined;
	let entry: Partial<WalterMieteEntry> | undefined = undefined;
	a?.then((e) => (entry = e));
</script>

<WalterDataWrapper
	{navigate}
	{addUrl}
	addEntry={entry}
	{title}
	{search}
	{rows}
	{headers}
>
	{#if entry}
		<WalterMiete {a} {entry} />
	{/if}
</WalterDataWrapper>
