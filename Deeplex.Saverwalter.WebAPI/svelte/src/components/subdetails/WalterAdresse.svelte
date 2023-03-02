<script lang="ts">
	import { Row } from 'carbon-components-svelte';

	import type { WalterAdresseEntry } from '$WalterTypes';
	import { WalterTextInput } from '$WalterComponents';

	export let adresse: Promise<WalterAdresseEntry> | undefined = undefined;

	// TODO Statt dass der Inhalt der Adresse sich ändert, muss sich die Adresse ändern.
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
