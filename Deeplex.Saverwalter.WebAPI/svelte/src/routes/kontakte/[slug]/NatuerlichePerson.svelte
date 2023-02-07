<script lang="ts">
	import { Column, Grid, Row, TextInput } from 'carbon-components-svelte';
	import Person from './Person.svelte';
	import type { NatuerlichePersonEntry } from './classes';

	export let person: NatuerlichePersonEntry;
</script>

<Grid padding>
	<Row>
		<TextInput labelText="Vorname" value={person.vorname} />
		<TextInput required labelText="Nachname" value={person.nachname} />
	</Row>
	<Person {person} />
</Grid>
