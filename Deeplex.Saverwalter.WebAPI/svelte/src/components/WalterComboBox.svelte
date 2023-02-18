<script lang="ts">
	import { ComboBox, SkeletonPlaceholder } from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';

	export let selectedId: string | undefined;
	export let titleText: string;
	export let items: ComboBoxItem[] | Promise<ComboBoxItem[]>;

	function shouldFilterItem(item: ComboBoxItem, value: string) {
		if (!value) return true;
		return item.text.toLowerCase().includes(value.toLowerCase());
	}

	const placeholderItems = Array(5)
		.fill(null)
		.map((e, i) => ({ id: i, text: 'undefined', disabled: true }));

	let ref: ComboBox;
	let backup: string | undefined = selectedId;

	function clear(_: FocusEvent) {
		ref.clear();
	}
</script>

{#await items}
	<ComboBox
		style="padding-right: 1rem"
		items={placeholderItems}
		{titleText}
		{selectedId}
		{shouldFilterItem}
	>
		<SkeletonPlaceholder style="width:100%" />
	</ComboBox>
{:then x}
	<ComboBox
		bind:selectedId
		on:blur={() => (selectedId = backup)}
		on:select={() => {
			backup = selectedId;
		}}
		on:focus={clear}
		bind:this={ref}
		style="padding-right: 1rem"
		items={x}
		{titleText}
		{shouldFilterItem}
	/>
{/await}
