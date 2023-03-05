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
	const url = `/api/wohnungen/${data.id}`;

	const a = data.a;

	const zaehlerEntry: Partial<WalterZaehlerEntry> = {
		wohnung: {
			id: '' + a.id,
			text: a.adresse?.anschrift + ' - ' + a.bezeichnung
		},
		adresse: { ...a.adresse }
	};
	const umlageEntry: Partial<WalterUmlageEntry> = {
		selectedWohnungen: [
			{
				id: '' + a.id,
				text: a.adresse?.anschrift + ' - ' + a.bezeichnung
			}
		]
	};
	const erhaltungsaufwendungEntry: Partial<WalterErhaltungsaufwendungEntry> = {
		wohnung: {
			id: '' + a.id,
			text: a.adresse?.anschrift + ' - ' + a.bezeichnung
		},
		datum: toLocaleIsoString(new Date())
	};
</script>

<WalterHeaderDetail
	{a}
	{url}
	title={a.adresse?.anschrift + ' - ' + a.bezeichnung}
/>

<WalterGrid>
	<WalterWohnung {a} />

	<Accordion>
		<WalterWohnungen title="Wohnungen an der selben Adresse" rows={a.haus} />
		<WalterZaehlerList a={zaehlerEntry} title="Zähler" rows={a.zaehler} />
		<WalterVertraege title="Verträge" rows={a.vertraege} />
		<WalterUmlagen a={umlageEntry} title="Umlagen" rows={a.umlagen} />
		<WalterBetriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.betriebskostenrechnungen}
		/>
		<WalterErhaltungsaufwendungen
			a={erhaltungsaufwendungEntry}
			title="Erhaltungsaufwendungen"
			rows={a.erhaltungsaufwendungen}
		/>
	</Accordion>
</WalterGrid>
