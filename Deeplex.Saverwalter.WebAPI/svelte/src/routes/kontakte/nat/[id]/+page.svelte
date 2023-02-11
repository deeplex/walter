<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import Person from '../../../../components/Person.svelte';
	import { Grid, Row, TextInput } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { NatuerlichePersonEntry } from '../../../../types/natuerlicheperson.type';
	import AsyncPerson from '../../../../components/AsyncPerson.svelte';
	import { request_options } from '../../../../services/utilts';

	export let data: PageData;
	const async: Promise<NatuerlichePersonEntry> = fetch(
		`/api/kontakte/nat/${data.id}`,
		request_options
	).then((e) => e.json());
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
