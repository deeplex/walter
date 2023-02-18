<script lang="ts">
	import { DatePicker, DatePickerInput } from 'carbon-components-svelte';
	import { convertDate } from '../services/utils';

	export let labelText: string;
	export let value: string | undefined;
	export let placeholder: string | undefined = undefined;
</script>

<DatePicker
	value={convertDate(value)}
	dateFormat="d.m.Y"
	datePickerType="single"
	on:change
>
	<DatePickerInput type="text" {placeholder} {labelText} />
</DatePicker>
