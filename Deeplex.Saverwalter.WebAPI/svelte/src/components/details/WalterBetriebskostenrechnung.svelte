<script lang="ts">
	import {
		WalterDatePicker,
		WalterNumberInput,
		WalterTextInput
	} from '$WalterComponents';
	import { Row } from 'carbon-components-svelte';
	import type { WalterBetriebskostenrechnungEntry } from '$WalterTypes';

	export let a: Promise<WalterBetriebskostenrechnungEntry> | undefined =
		undefined;
	export let entry: Partial<WalterBetriebskostenrechnungEntry> = {};
</script>

<Row>
	<!-- TODO bind:binding={entry.umlage?.typ} -->
	<WalterTextInput labelText="Typ" value={a?.then((x) => x.umlage.text)} />
	<!-- TODO bind:binding={entry.wohnungenBezeichnung} -->
	<WalterTextInput
		labelText="Wohnungen"
		value={a?.then((x) => x.wohnungenBezeichnung)}
	/>
</Row>
<Row>
	<WalterNumberInput
		bind:binding={entry.betreffendesJahr}
		hideSteppers={false}
		label="Betreffendes Jahr"
		value={a?.then((x) => x.betreffendesJahr)}
	/>
	<WalterNumberInput
		bind:binding={entry.betrag}
		label="Betrag"
		value={a?.then((x) => x.betrag)}
	/>
	<WalterDatePicker
		bind:binding={entry.datum}
		labelText="Datum"
		value={a?.then((x) => x.datum)}
	/>
</Row>
