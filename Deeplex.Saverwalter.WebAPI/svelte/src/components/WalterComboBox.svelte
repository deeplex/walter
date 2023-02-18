<script lang="ts">
	import { ComboBox, TextInputSkeleton } from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';

	export let selectedId: Promise<string> | undefined;
	export let titleText: string;
	export let items: ComboBoxItem[] | Promise<ComboBoxItem[]>;

	function shouldFilterItem(item: ComboBoxItem, value: string) {
		if (!value) return true;
		return item.text.toLowerCase().includes(value.toLowerCase());
	}

	let ref: ComboBox;
	let backup: Promise<string> | undefined = selectedId;

	function clear(_: FocusEvent) {
		ref.clear();
	}
</script>

{#await items}
	<TextInputSkeleton />
{:then y}
	{#await selectedId}
		<TextInputSkeleton />
	{:then x}
		<ComboBox
			selectedId={x}
			on:blur={() => (selectedId = backup)}
			on:select={() => {
				backup = selectedId;
			}}
			on:focus={clear}
			bind:this={ref}
			style="padding-right: 1rem"
			items={y}
			{titleText}
			{shouldFilterItem}
		/>
	{/await}
{/await}
