<script lang="ts">
	import { Column, Row, TextInput } from 'carbon-components-svelte';
	import type { Adresse } from './adresse';

	export let adresse: Adresse;
</script>

<Row>
	<TextInput labelText="Straße" value={adresse.strasse} />
	<TextInput labelText="Hausnr." value={adresse.hausnummer} />
	<TextInput labelText="Postleitzahl" value={adresse.postleitzahl} />
	<TextInput labelText="Stadt" value={adresse.stadt} />
</Row>
