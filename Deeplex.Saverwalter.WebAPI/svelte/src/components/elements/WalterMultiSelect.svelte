<script lang="ts">
	import type { WalterSelectionEntry } from '$WalterLib';
	import { MultiSelect, TextInputSkeleton } from 'carbon-components-svelte';

	export let value: WalterSelectionEntry[] | undefined;
	export let titleText: string;
	export let a: WalterSelectionEntry[];

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
			placeholder={value?.map((e) => e.text).join(', ')}
			selectedIds={x?.map((e) => e.id)}
			style="padding-right: 1rem"
			{items}
			on:select={select}
			{titleText}
			filterable
		/>
	{/await}
{/await}
