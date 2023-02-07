<script lang="ts">
	import {
		DatePicker,
		DatePickerInput,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import { VertragEntry } from './classes';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const async = fetch(`/api/vertraege/${window.location.href.split('/').at(-1)}`, request_options)
		.then((e) => e.json())
		.then((e) => new VertragEntry(e));
</script>

<Grid>
	{#await async}
		<Row>
			<TextInputSkeleton />
		</Row>
	{:then x}
		<Row>
			<DatePicker value={x.beginn} dateFormat="d.m.Y" datePickerType="single" on:change>
				<DatePickerInput type="text" labelText="Beginn" required />
			</DatePicker>
			<DatePicker value={x.ende} dateFormat="d.m.Y" datePickerType="single" on:change>
				<DatePickerInput type="text" labelText="Ende" placeholder="Offen" />
			</DatePicker>
		</Row>
	{/await}
</Grid>
