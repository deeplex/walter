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
	import { walter_get } from '$WalterServices/requests';
	import type {
		WalterBetriebskostenrechnungEntry,
		WalterSelectionEntry,
		WalterUmlageEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';
	import WalterBetriebskostenrechnung from '../../../components/details/WalterBetriebskostenrechnung.svelte';

	export let data: PageData;
	const url = `/api/umlagen/${data.id}`;

	const a = data.a;

	const lastBetriebskostenrechnung =
		a.betriebskostenrechnungen[a.betriebskostenrechnungen.length - 1];

	const umlage: WalterSelectionEntry = {
		id: '' + a.id,
		text: a.wohnungenBezeichnung,
		filter: '' + a.typ
	};
	const betriebskostenrechungEntry: Partial<WalterBetriebskostenrechnungEntry> =
		{
			typ: a.typ,
			umlage: umlage,
			betrag: lastBetriebskostenrechnung.betrag,
			betreffendesJahr: lastBetriebskostenrechnung.betreffendesJahr + 1,
			datum: toLocaleIsoString(new Date())
		};
</script>

<WalterHeaderDetail
	{a}
	{url}
	title={a.typ.text + ' - ' + a.wohnungenBezeichnung}
/>

<WalterGrid>
	<WalterUmlage {a} />

	<Accordion>
		<WalterWohnungen title="Wohnungen" rows={a.wohnungen} />
		<WalterBetriebskostenrechnungen
			a={betriebskostenrechungEntry}
			title="Betriebskostenrechnungen"
			rows={a.betriebskostenrechnungen}
		/>
	</Accordion>
</WalterGrid>
