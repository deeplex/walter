<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import {
		Anhaenge,
		WalterDatePicker,
		WalterGrid,
		WalterHeader,
		WalterTextInput,
		WalterNumberInput,
		Wohnungen
	} from '../../../components';
	import { walter_get } from '../../../services/utils';
	import type { BetriebskostenrechnungEntry } from '../../../types/betriebskostenrechnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<BetriebskostenrechnungEntry> = walter_get(
		`/api/betriebskostenrechnungen/${data.id}`
	);
	const entry: Partial<BetriebskostenrechnungEntry> = {};
	a.then((e) => Object.assign(entry, e));
</script>

<WalterHeader
	title={a.then(
		(x) =>
			x.betreffendesJahr +
			' - TODO - ' +
			x.umlage.text +
			' - TODO - ' +
			x.umlage.text
	)}
>
	<Anhaenge rows={a.then((x) => x.anhaenge)} />
</WalterHeader>

<WalterGrid>
	<Row>
		<!-- TODO bind:binding={entry.umlage?.typ} -->
		<WalterTextInput labelText="Typ" value={a.then((x) => x.umlage.text)} />
		<!-- TODO bind:binding={entry.wohnungenBezeichnung} -->
		<WalterTextInput
			labelText="Wohnungen"
			value={a.then((x) => x.umlage.text)}
		/>
	</Row>
	<Row>
		<WalterNumberInput
			bind:binding={entry.betreffendesJahr}
			hideSteppers={false}
			label="Betreffendes Jahr"
			value={a.then((x) => x.betreffendesJahr)}
		/>
		<WalterNumberInput
			bind:binding={entry.betrag}
			label="Betrag"
			value={a.then((x) => x.betrag)}
		/>
		<WalterDatePicker
			bind:binding={entry.datum}
			labelText="Datum"
			value={a.then((x) => x.datum)}
		/>
	</Row>

	<Accordion>
		<Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
	</Accordion>
</WalterGrid>
