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
	import { toLocaleIsoString } from '$WalterServices/utils';
	import type {
		WalterBetriebskostenrechnungEntry,
		WalterSelectionEntry
	} from '$WalterLib';

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
	S3URL={data.S3URL}
	files={data.anhaenge}
	a={data.a}
	apiURL={data.apiURL}
	title={`${data.a.typ.text} - ${data.a.wohnungenBezeichnung}`}
/>

<WalterGrid>
	<WalterUmlage
		betriebskostentypen={data.betriebskostentypen}
		wohnungen={data.wohnungen}
		umlageschluessel={data.umlageschluessel}
		a={data.a}
	/>

	<Accordion>
		<WalterWohnungen
			kontakte={data.kontakte}
			title="Wohnungen"
			rows={data.a.wohnungen}
		/>
		<WalterBetriebskostenrechnungen
			betriebskostentypen={data.betriebskostentypen}
			umlagen={data.umlagen}
			a={betriebskostenrechungEntry}
			title="Betriebskostenrechnungen"
			rows={data.a.betriebskostenrechnungen}
		/>
	</Accordion>
</WalterGrid>
