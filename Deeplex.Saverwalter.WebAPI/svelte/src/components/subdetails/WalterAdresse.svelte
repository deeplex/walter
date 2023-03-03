<script lang="ts">
	import { Row } from 'carbon-components-svelte';

	import type { WalterAdresseEntry } from '$WalterTypes';
	import { WalterTextInput } from '$WalterComponents';

	export let adresse: Promise<WalterAdresseEntry> | undefined = undefined;
	export let entry: Partial<WalterAdresseEntry> = {};
	adresse?.then((e) => Object.assign(entry, e));
</script>

<Row>
	<WalterTextInput
		labelText="Straße"
		bind:binding={entry.strasse}
		value={adresse?.then((x) => x?.strasse)}
	/>
	<WalterTextInput
		labelText="Hausnr."
		bind:binding={entry.hausnummer}
		value={adresse?.then((x) => x?.hausnummer)}
	/>
	<WalterTextInput
		labelText="Postleitzahl"
		bind:binding={entry.postleitzahl}
		value={adresse?.then((x) => x?.postleitzahl)}
	/>
	<WalterTextInput
		labelText="Stadt"
		bind:binding={entry.stadt}
		value={adresse?.then((x) => x?.stadt)}
	/>
</Row>
