<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		WalterGrid,
		WalterWohnungen,
		WalterVertraege,
		WalterZaehlerList,
		WalterErhaltungsaufwendungen,
		WalterBetriebskostenrechnungen,
		WalterUmlagen,
		WalterHeaderDetail,
		WalterWohnung
	} from '$WalterComponents';
	import type {
		WalterErhaltungsaufwendungEntry,
		WalterUmlageEntry,
		WalterZaehlerEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';

	export let data: PageData;

	const zaehlerEntry: Partial<WalterZaehlerEntry> = {
		wohnung: {
			id: '' + data.a.id,
			text: data.a.adresse?.anschrift + ' - ' + data.a.bezeichnung
		},
		adresse: { ...data.a.adresse }
	};
	const umlageEntry: Partial<WalterUmlageEntry> = {
		selectedWohnungen: [
			{
				id: '' + data.a.id,
				text: data.a.adresse?.anschrift + ' - ' + data.a.bezeichnung
			}
		]
	};
	const erhaltungsaufwendungEntry: Partial<WalterErhaltungsaufwendungEntry> = {
		wohnung: {
			id: '' + data.a.id,
			text: data.a.adresse?.anschrift + ' - ' + data.a.bezeichnung
		},
		datum: toLocaleIsoString(new Date())
	};
</script>

<WalterHeaderDetail
	a={data.a}
	url={data.url}
	title={data.a.adresse?.anschrift + ' - ' + data.a.bezeichnung}
/>

<WalterGrid>
	<WalterWohnung a={data.a} />

	<Accordion>
		<WalterWohnungen
			title="Wohnungen an der selben Adresse"
			rows={data.a.haus}
		/>
		<WalterZaehlerList a={zaehlerEntry} title="Zähler" rows={data.a.zaehler} />
		<WalterVertraege title="Verträge" rows={data.a.vertraege} />
		<WalterUmlagen a={umlageEntry} title="Umlagen" rows={data.a.umlagen} />
		<WalterBetriebskostenrechnungen
			betriebskostentypen={data.betriebskostentypen}
			umlagen={data.umlagen}
			title="Betriebskostenrechnungen"
			rows={data.a.betriebskostenrechnungen}
		/>
		<WalterErhaltungsaufwendungen
			a={erhaltungsaufwendungEntry}
			title="Erhaltungsaufwendungen"
			rows={data.a.erhaltungsaufwendungen}
		/>
	</Accordion>
</WalterGrid>
