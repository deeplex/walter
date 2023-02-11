<script lang="ts">
	import { Row, TextInput } from 'carbon-components-svelte';
	import type { PersonEntry } from '../types/person.type';
	import Adresse from './Adresse.svelte';

	export let person: PersonEntry;
</script>

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
