<script lang="ts">
	import {
		DatePicker,
		DatePickerInput,
		DatePickerSkeleton
	} from 'carbon-components-svelte';
	import { convertDate } from '../services/utils';

	export let labelText: string;
	export let value: Promise<string | undefined> | undefined;
	export let binding: string | undefined = undefined;
	export let placeholder: string | undefined = undefined;

	function change(e: any) {
		binding = e.detail?.dateStr;
	}
</script>

{#await value}
	<DatePickerSkeleton />
{:then x}
	<DatePicker
		value={convertDate(x)}
		dateFormat="d.m.Y"
		datePickerType="single"
		on:change={change}
	>
		<DatePickerInput
			style="width: 100%"
			type="text"
			{placeholder}
			{labelText}
		/>
	</DatePicker>
{/await}
