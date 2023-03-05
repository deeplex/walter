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

	export let rows: WalterMietminderungEntry[];
	export let search: boolean = false;
	export let title: string | undefined = undefined;

	const navigate = (e: CustomEvent) => goto(`/mietminderungen/${e.detail.id}`);

	export let a: Partial<WalterMietminderungEntry> | undefined = undefined;
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
		<WalterMietminderung {a} />
	{/if}
</WalterDataWrapper>
