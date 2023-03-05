<script lang="ts">
	import {
		WalterAdresse,
		WalterComboBox,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterSelectionEntry, WalterWohnungEntry } from '$WalterTypes';

	export let a: Partial<WalterWohnungEntry> = {};
	export let kontakte: WalterSelectionEntry[];
</script>

<Row>
	<WalterComboBox bind:value={a.besitzer} titleText="Besitzer" a={kontakte} />
</Row>
<WalterAdresse bind:value={a.adresse} />
<Row>
	<WalterTextInput bind:value={a.bezeichnung} labelText="Bezeichnung" />
	<WalterNumberInput bind:value={a.wohnflaeche} label="Wohnfläche" />
	<WalterNumberInput bind:value={a.nutzflaeche} label="Nutzfläche" />
	<WalterNumberInput
		hideSteppers={false}
		bind:value={a.einheiten}
		label="Einheiten"
	/>
</Row>
<Row>
	<WalterTextInput bind:value={a.notiz} labelText="Notiz" />
</Row>
