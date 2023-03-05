<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';
	import {
		WalterBetriebskostenrechnungen,
		WalterHeaderDetail,
		WalterGrid,
		WalterWohnungen,
		WalterUmlage
	} from '$WalterComponents';
	import type {
		WalterBetriebskostenrechnungEntry,
		WalterSelectionEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';

	export let data: PageData;

	const lastBetriebskostenrechnung =
		data.a.betriebskostenrechnungen[data.a.betriebskostenrechnungen.length - 1];

	const umlage: WalterSelectionEntry = {
		id: '' + data.a.id,
		text: data.a.wohnungenBezeichnung,
		filter: '' + data.a.typ
	};
	const betriebskostenrechungEntry: Partial<WalterBetriebskostenrechnungEntry> =
		{
			typ: data.a.typ,
			umlage: umlage,
			betrag: lastBetriebskostenrechnung.betrag,
			betreffendesJahr: lastBetriebskostenrechnung.betreffendesJahr + 1,
			datum: toLocaleIsoString(new Date())
		};
</script>

<WalterHeaderDetail
	a={data.a}
	url={data.url}
	title={data.a.typ.text + ' - ' + data.a.wohnungenBezeichnung}
/>

<WalterGrid>
	<WalterUmlage a={data.a} />

	<Accordion>
		<WalterWohnungen title="Wohnungen" rows={data.a.wohnungen} />
		<WalterBetriebskostenrechnungen
			a={betriebskostenrechungEntry}
			title="Betriebskostenrechnungen"
			rows={data.a.betriebskostenrechnungen}
		/>
	</Accordion>
</WalterGrid>
