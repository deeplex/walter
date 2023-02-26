<script lang="ts">
	import { Row } from 'carbon-components-svelte';
	import type { AdresseEntry } from '../types/adresse.type';
	import WalterTextInput from './WalterTextInput.svelte';

	export let adresse: Promise<AdresseEntry>;
</script>

<Row>
	<WalterTextInput
		labelText="Straße"
		value={adresse?.then((x) => x?.strasse)}
	/>
	<WalterTextInput
		labelText="Hausnr."
		value={adresse?.then((x) => x?.hausnummer)}
	/>
	<WalterTextInput
		labelText="Postleitzahl"
		value={adresse?.then((x) => x?.postleitzahl)}
	/>
	<WalterTextInput labelText="Stadt" value={adresse?.then((x) => x?.stadt)} />
</Row>
