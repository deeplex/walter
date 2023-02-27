<script lang="ts">
	import {
		ComboBox,
		Select,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import type { ComboBoxItem } from 'carbon-components-svelte/types/ComboBox/ComboBox.svelte';
	import { walter_get } from '../services/utils';
	import type { SelectionEntry } from '../types/selection.type';

	type Selection = { selectedId: any; selectedItem: ComboBoxItem };

	export let value: Promise<SelectionEntry | undefined> | undefined;
	export let titleText: string;
	export let binding: SelectionEntry | undefined = undefined;
	export let api: string;

	const a: Promise<SelectionEntry[]> = walter_get(api);

	function shouldFilterItem(item: ComboBoxItem, value: string) {
		if (!value) return true;
		return item.text.toLowerCase().includes(value.toLowerCase());
	}

	function select(e: CustomEvent<Selection>) {
		binding = e.detail.selectedItem;
	}
</script>

{#await a}
	<TextInputSkeleton />
{:then items}
	{#await value}
		<TextInputSkeleton />
	{:then x}
		<ComboBox
			selectedId={x?.id}
			on:select={select}
			style="padding-right: 1rem"
			{items}
			value={x?.text}
			{titleText}
			{shouldFilterItem}
		/>
	{/await}
{/await}
