<script lang="ts">
	import { goto } from '$app/navigation';
	import { WalterDataWrapper, WalterZaehlerstand } from '$WalterComponents';

	import type { WalterZaehlerstandEntry } from '$WalterTypes';

	const headers = [
		{ key: 'datum', value: 'Datum' },
		{ key: 'stand', value: 'Stand' },
		{ key: 'einheit', value: 'Einheit' }
	];

	const addUrl = `/api/zaehlerstaende/`;

	export let rows: Promise<WalterZaehlerstandEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	const navigate = (e: CustomEvent) => goto(`/zaehlerstaende/${e.detail.id}`);

	export let a: Promise<Partial<WalterZaehlerstandEntry>> | undefined =
		undefined;
	let entry: Partial<WalterZaehlerstandEntry> | undefined = undefined;
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
		<WalterZaehlerstand {a} {entry} />
	{/if}
</WalterDataWrapper>
