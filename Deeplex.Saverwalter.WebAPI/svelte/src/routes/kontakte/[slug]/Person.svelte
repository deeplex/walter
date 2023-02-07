<script lang="ts">
	import { Column, Row, TextInput } from 'carbon-components-svelte';
	import Adresse from '../../../components/adresse.svelte';
	import type { PersonEntry } from './classes';

	export let person: PersonEntry;
</script>

<div>
	<Adresse adresse={person.adresse} />
	<Row>
		<TextInput labelText="E-Mail" value={person.email} />
		<TextInput labelText="Fax" value={person.fax} />
	</Row>
	<Row>
		<TextInput labelText="Telefon" value={person.telefon} />
		<TextInput labelText="Mobil" value={person.mobil} />
	</Row>
	<Row>
		<TextInput labelText="Notiz" value={person.notiz} />
	</Row>
</div>
