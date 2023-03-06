<script lang="ts">
	import {
		DatePicker,
		DatePickerInput,
		DatePickerSkeleton
	} from 'carbon-components-svelte';

	import { convertDate, toLocaleIsoString } from '$WalterServices/utils';

	export let labelText: string;
	export let value: string | undefined = undefined;
	export let placeholder: string | undefined = undefined;
	export let disabled: boolean | undefined = false;

	function change(e: any) {
		value = toLocaleIsoString(new Date(e.detail?.selectedDates[0]));
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
			{disabled}
			style="width: 100%"
			type="text"
			{placeholder}
			{labelText}
		/>
	</DatePicker>
{/await}
