<script lang="ts">
	import {
		DatePicker,
		DatePickerInput,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import { convertDate } from '../../../services/utilts';
	import type { VertragEntry } from '../../../types/vertrag.type';
	import type { PageData } from './$types';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	export let data: PageData;
	const async: Promise<VertragEntry> = fetch(`/api/vertraege/${data.id}`, request_options).then(
		(e) => e.json()
	);
</script>

<Grid>
	{#await async}
		<Row>
			<TextInputSkeleton />
		</Row>
	{:then x}
		<Row>
			<DatePicker
				value={convertDate(x.beginn)}
				dateFormat="d.m.Y"
				datePickerType="single"
				on:change
			>
				<DatePickerInput type="text" labelText="Beginn" required />
			</DatePicker>
			<DatePicker value={convertDate(x.ende)} dateFormat="d.m.Y" datePickerType="single" on:change>
				<DatePickerInput type="text" labelText="Ende" placeholder="Offen" />
			</DatePicker>
		</Row>
	{/await}
</Grid>
