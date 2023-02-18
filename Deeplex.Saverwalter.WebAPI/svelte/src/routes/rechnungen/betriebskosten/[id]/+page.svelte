<script lang="ts">
	import {
		DatePicker,
		DatePickerInput,
		Grid,
		Row,
		TextInput,
		TextInputSkeleton
	} from 'carbon-components-svelte';
	import WalterDatePicker from '../../../../components/WalterDatePicker.svelte';
	import WalterGrid from '../../../../components/WalterGrid.svelte';
	import WalterHeader from '../../../../components/WalterHeader.svelte';
	import WalterTextInput from '../../../../components/WalterTextInput.svelte';
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
</WalterGrid>
