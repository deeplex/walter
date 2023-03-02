<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import {
		WalterDatePicker,
		WalterGrid,
		WalterTextInput,
		WalterNumberInput,
		Wohnungen,
		WalterDetailHeader
	} from '$components';
	import { walter_get } from '$services/utils';
	import type { BetriebskostenrechnungEntry } from '$types';

	export let data: PageData;
	const url = `/api/betriebskostenrechnungen/${data.id}`;

	const a: Promise<BetriebskostenrechnungEntry> = walter_get(url);
	const entry: Partial<BetriebskostenrechnungEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then(
		(x) =>
			x.betreffendesJahr +
			' - TODO - ' +
			x.umlage.text +
			' - TODO - ' +
			x.umlage.text
	);
</script>

<WalterDetailHeader {a} {url} {entry} {title} />

<WalterGrid>
	<Row>
		<!-- TODO bind:binding={entry.umlage?.typ} -->
		<WalterTextInput labelText="Typ" value={a.then((x) => x.umlage.text)} />
		<!-- TODO bind:binding={entry.wohnungenBezeichnung} -->
		<WalterTextInput
			labelText="Wohnungen"
			value={a.then((x) => x.wohnungenBezeichnung)}
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
