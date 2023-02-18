<script lang="ts">
	import { Row, TextInput } from 'carbon-components-svelte';
	import type { PersonEntry } from '../types/person.type';
	import Adresse from './Adresse.svelte';
	import WalterTextInput from './WalterTextInput.svelte';

	export let person: Promise<PersonEntry>;
</script>

<Adresse adresse={person.then((e) => e.adresse)} />
<Row>
	<WalterTextInput labelText="E-Mail" value={person.then((e) => e.email)} />
	<WalterTextInput labelText="Fax" value={person.then((e) => e.fax)} />
</Row>
<Row>
	<WalterTextInput labelText="Telefon" value={person.then((e) => e.telefon)} />
	<WalterTextInput labelText="Mobil" value={person.then((e) => e.mobil)} />
</Row>
<Row>
	<WalterTextInput labelText="Notiz" value={person.then((e) => e.notiz)} />
</Row>
