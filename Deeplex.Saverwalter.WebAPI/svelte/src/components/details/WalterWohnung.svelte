<script lang="ts">
	import {
		Adresse,
		WalterComboBox,
		WalterNumberInput,
		WalterTextInput
	} from '$components';
	import { Row } from 'carbon-components-svelte';
	import type { WohnungEntry } from '$types';

	export let a: Promise<WohnungEntry> | undefined = undefined;
	export let entry: Partial<WohnungEntry> = {};
</script>

<Row>
	<WalterComboBox
		bind:binding={entry.besitzer}
		titleText="Besitzer"
		api={`/api/selection/kontakte`}
		value={a?.then((x) => x.besitzer)}
	/>
</Row>
<!-- TODO -->
<Adresse adresse={a?.then((x) => x.adresse)} />
<Row>
	<WalterTextInput
		bind:binding={entry.bezeichnung}
		labelText="Bezeichnung"
		value={a?.then((x) => x.bezeichnung)}
	/>
	<WalterNumberInput
		bind:binding={entry.wohnflaeche}
		label="Wohnfläche"
		value={a?.then((x) => x.wohnflaeche)}
	/>
	<WalterNumberInput
		bind:binding={entry.nutzflaeche}
		label="Nutzfläche"
		value={a?.then((x) => x.nutzflaeche)}
	/>
	<WalterNumberInput
		hideSteppers={false}
		bind:binding={entry.einheiten}
		label="Einheiten"
		value={a?.then((x) => x.einheiten)}
	/>
</Row>
<Row>
	<WalterTextInput
		bind:binding={entry.notiz}
		labelText="Notiz"
		value={a?.then((x) => x.notiz)}
	/>
</Row>
