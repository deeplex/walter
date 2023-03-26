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
	import { toLocaleIsoString } from '$WalterServices/utils';
	import type {
		WalterErhaltungsaufwendungEntry,
		WalterUmlageEntry,
		WalterZaehlerEntry
	} from '$WalterLib';

	export let data: PageData;

	const zaehlerEntry: Partial<WalterZaehlerEntry> = {
		wohnung: {
			id: `${data.a.id}`,
			text: `${data.a.adresse?.anschrift} - ${data.a.bezeichnung}`
		},
		adresse: data.a.adresse ? { ...data.a.adresse } : undefined
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
	S3URL={data.S3URL}
	files={data.anhaenge}
	a={data.a}
	apiURL={data.apiURL}
	title={data.a.adresse?.anschrift + ' - ' + data.a.bezeichnung}
/>

<WalterGrid>
	<WalterWohnung kontakte={data.kontakte} a={data.a} />

	<Accordion>
		<WalterWohnungen
			kontakte={data.kontakte}
			title="Wohnungen an der selben Adresse"
			rows={data.a.haus || []}
		/>
		<WalterZaehlerList
			zaehlertypen={data.zaehlertypen}
			zaehler={data.zaehler}
			wohnungen={data.wohnungen}
			a={zaehlerEntry}
			title="Zähler"
			rows={data.a.zaehler}
		/>
		<WalterVertraege
			wohnungen={data.wohnungen}
			kontakte={data.kontakte}
			title="Verträge"
			rows={data.a.vertraege}
		/>
		<WalterUmlagen
			wohnungen={data.wohnungen}
			umlageschluessel={data.umlageschluessel}
			betriebskostentypen={data.betriebskostentypen}
			a={umlageEntry}
			title="Umlagen"
			rows={data.a.umlagen}
		/>
		<WalterBetriebskostenrechnungen
			betriebskostentypen={data.betriebskostentypen}
			umlagen={data.umlagen}
			title="Betriebskostenrechnungen"
			rows={data.a.betriebskostenrechnungen}
		/>
		<WalterErhaltungsaufwendungen
			wohnungen={data.wohnungen}
			kontakte={data.kontakte}
			a={erhaltungsaufwendungEntry}
			title="Erhaltungsaufwendungen"
			rows={data.a.erhaltungsaufwendungen}
		/>
	</Accordion>
</WalterGrid>
