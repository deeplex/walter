<script lang="ts">
	import {
		DatePicker,
		DatePickerInput,
		DatePickerSkeleton
	} from 'carbon-components-svelte';
	import { convertDate } from '../services/utils';

	export let labelText: string;
	export let value: Promise<string | undefined> | string | undefined;
	export let placeholder: string | undefined = undefined;
</script>

{#await value}
	<DatePickerSkeleton />
{:then x}
	<DatePicker
		value={convertDate(x)}
		dateFormat="d.m.Y"
		datePickerType="single"
		on:change
	>
		<DatePickerInput
			style="width: 100%"
			type="text"
			{placeholder}
			{labelText}
		/>
	</DatePicker>
{/await}
