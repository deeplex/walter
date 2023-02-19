<script lang="ts">
	import { Accordion, Row } from 'carbon-components-svelte';
	import {
		WalterDatePicker,
		WalterGrid,
		WalterHeader,
		WalterTextInput,
		Wohnungen
	} from '../../../../components';
	import { walter_get } from '../../../../services/utils';
	import type { BetriebskostenrechnungEntry } from '../../../../types/betriebskostenrechnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const a: Promise<BetriebskostenrechnungEntry> = walter_get(
		`/api/betriebskostenrechnungen/${data.id}`
	);
</script>

<WalterHeader
	title={a.then(
		(x) =>
			x.betreffendesJahr +
			' - ' +
			x.umlage.typ +
			' - ' +
			x.umlage.wohnungenBezeichnung
	)}
/>

<WalterGrid>
	<Row>
		<WalterTextInput labelText="Typ" value={a.then((x) => x.umlage.typ)} />
		<WalterTextInput
			labelText="Wohnungen"
			value={a.then((x) => x.umlage.wohnungenBezeichnung)}
		/>
	</Row>
	<Row>
		<WalterTextInput
			labelText="Betreffendes Jahr"
			value={a.then((x) => x.betreffendesJahr)}
		/>
		<WalterTextInput labelText="Betrag" value={a.then((x) => x.betrag)} />
		<WalterDatePicker labelText="Datum" value={a.then((x) => x.datum)} />
	</Row>

	<Accordion>
		<Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
	</Accordion>
</WalterGrid>
