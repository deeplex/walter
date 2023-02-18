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
	import { request_options } from '../../../../services/utils';
	import type { BetriebskostenrechnungEntry } from '../../../../types/betriebskostenrechnung.type';
	import type { PageData } from './$types';

	export let data: PageData;
	const async: Promise<BetriebskostenrechnungEntry> = fetch(
		`/api/betriebskostenrechnungen/${data.id}`,
		request_options
	).then((e) => e.json());
</script>

<Grid>
	{#await async}
		<TextInputSkeleton />
	{:then x}
		<Row>
			<TextInput labelText="Typ" value={x.umlage.typ} />
			<TextInput labelText="Wohnungen" value={x.umlage.wohnungenBezeichnung} />
		</Row>
		<Row>
			<TextInput labelText="Betreffendes Jahr" value={x.betreffendesJahr} />
			<TextInput labelText="Betrag" value={x.betrag} />
			<WalterDatePicker value={x.datum} labelText="Datum" />
		</Row>
	{/await}
</Grid>
