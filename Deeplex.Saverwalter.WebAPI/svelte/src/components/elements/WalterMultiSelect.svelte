<script lang="ts">
	import { walter_get } from '$WalterServices/requests';
	import type { WalterSelectionEntry } from '$WalterTypes';
	import { MultiSelect, TextInputSkeleton } from 'carbon-components-svelte';

	export let value: WalterSelectionEntry[] | undefined;
	export let titleText: string;
	export let api: string;

	const a: Promise<WalterSelectionEntry[]> = walter_get(api);

	function select(e: CustomEvent) {
		value = e.detail.selected;
	}
</script>

{#await a}
	<TextInputSkeleton />
{:then items}
	{#await value}
		<TextInputSkeleton />
	{:then x}
		<MultiSelect
			selectedIds={x?.map((e) => e.id)}
			style="padding-right: 1rem"
			{items}
			on:select={select}
			{titleText}
			filterable
		/>
	{/await}
{/await}
