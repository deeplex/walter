<script lang="ts">
	import { goto } from '$app/navigation';
	import { WalterDataWrapper, WalterMietminderung } from '$WalterComponents';
	import type { WalterMietminderungEntry } from '$WalterTypes';

	const headers = [
		{ key: 'beginn', value: 'Beginn' },
		{ key: 'ende', value: 'Ende' },
		{ key: 'minderung', value: 'Minderung' }
	];

	const addUrl = `/api/mietminderungen/`;

	export let rows: Promise<WalterMietminderungEntry[]>;
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	const navigate = (e: CustomEvent) => goto(`/mietminderungen/${e.detail.id}`);

	export let a: Promise<Partial<WalterMietminderungEntry>> | undefined =
		undefined;
	let entry: Partial<WalterMietminderungEntry> | undefined = undefined;
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
		<WalterMietminderung {a} {entry} />
	{/if}
</WalterDataWrapper>
