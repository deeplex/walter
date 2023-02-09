<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { JuristischePersonEntry } from './classes';
	import AsyncPerson from '../../person/AsyncPerson.svelte';
	import Person from '../../person/Person.svelte';
	import { Column, Grid, Row, TextInput, TextInputSkeleton } from 'carbon-components-svelte';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const async = fetch(
		`/api/kontakte/jur/${window.location.href.split('/').at(-1)}`,
		request_options
	)
		.then((e) => e.json())
		.then((p) => new JuristischePersonEntry(p));
</script>

<Grid padding>
	{#await async}
		<Row>
			<Column><TextInputSkeleton /></Column>
		</Row>
		<AsyncPerson />
	{:then person}
		<Row>
			<TextInput required labelText="Bezeichnung" value={person.name} />
		</Row>
		<Person {person} />
	{/await}
</Grid>
