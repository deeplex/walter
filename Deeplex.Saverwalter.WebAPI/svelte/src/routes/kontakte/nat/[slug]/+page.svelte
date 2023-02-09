<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { NatuerlichePersonEntry } from './classes';
	import Person from '../../person/Person.svelte';
	import AsyncPerson from '../../person/AsyncPerson.svelte';
	import { Grid, Row, TextInput } from 'carbon-components-svelte';

	const request_options = {
		method: 'GET',
		headers: {
			'Content-Type': 'text/json'
		}
	};

	const async = fetch(
		`/api/kontakte/nat/${window.location.href.split('/').at(-1)}`,
		request_options
	)
		.then((e) => e.json())
		.then((p) => new NatuerlichePersonEntry(p));
</script>

<Grid>
	{#await async}
		<AsyncPerson />
	{:then person}
		<Row>
			<TextInput labelText="Vorname" value={person.vorname} />
			<TextInput required labelText="Nachname" value={person.nachname} />
		</Row>
		<Person {person} />
	{/await}
</Grid>
