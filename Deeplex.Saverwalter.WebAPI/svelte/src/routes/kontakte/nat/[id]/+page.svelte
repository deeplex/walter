<script lang="ts">
	import Person from '../../../../components/Person.svelte';
	import { Grid, Row, TextInput } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import type { NatuerlichePersonEntry } from '../../../../types/natuerlicheperson.type';
	import AsyncPerson from '../../../../components/AsyncPerson.svelte';
	import { walter_get } from '../../../../services/utils';

	export let data: PageData;
	const async: Promise<NatuerlichePersonEntry> = walter_get(
		`/api/kontakte/nat/${data.id}`
	);
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
