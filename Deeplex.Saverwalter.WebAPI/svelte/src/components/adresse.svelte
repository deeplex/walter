<script lang="ts">
	import { Row, TextInput } from 'carbon-components-svelte';
	import type { AdresseEntry } from '../types/adresse.type';

	export let adresse: AdresseEntry;
</script>

<Row>
	<TextInput labelText="Straße" value={adresse?.strasse} />
	<TextInput labelText="Hausnr." value={adresse?.hausnummer} />
	<TextInput labelText="Postleitzahl" value={adresse?.postleitzahl} />
	<TextInput labelText="Stadt" value={adresse?.stadt} />
</Row>
