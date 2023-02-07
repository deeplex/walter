<script lang="ts">
	import { Column, Grid, Row, TextInput } from 'carbon-components-svelte';
	import Person from './Person.svelte';
	import type { JuristischePersonEntry } from './classes';

	export let person: JuristischePersonEntry;
</script>

<Grid padding>
	<Row>
		<TextInput required labelText="Bezeichnung" value={person.name} />
	</Row>
	<Person {person} />
</Grid>
