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
		WalterWohnungEntry,
		WalterZaehlerEntry
	} from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';
	import { toLocaleIsoString } from '$WalterServices/utils';

	export let data: PageData;
	const url = `/api/wohnungen/${data.id}`;

	const a: Promise<WalterWohnungEntry> = walter_get(url);
	const entry: Partial<WalterWohnungEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.adresse.anschrift + ' - ' + x.bezeichnung);

	const zaehlerEntry: Promise<Partial<WalterZaehlerEntry>> = a.then((e) => ({
		wohnung: {
			id: '' + e.id,
			text: e.adresse.anschrift + ' - ' + e.bezeichnung
		},
		adresse: { ...e.adresse }
	}));
	const umlageEntry: Promise<Partial<WalterUmlageEntry>> = a.then((e) => ({
		selectedWohnungen: [
			{
				id: '' + e.id,
				text: e.adresse.anschrift + ' - ' + e.bezeichnung
			}
		]
	}));
	const erhaltungsaufwendungEntry: Promise<
		Partial<WalterErhaltungsaufwendungEntry>
	> = a.then((e) => ({
		wohnung: {
			id: '' + e.id,
			text: e.adresse.anschrift + ' - ' + e.bezeichnung
		},
		datum: toLocaleIsoString(new Date())
	}));
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterWohnung {a} {entry} />

	<Accordion>
		<WalterWohnungen
			title="Wohnungen an der selben Adresse"
			rows={a.then((x) => x.haus)}
		/>
		<WalterZaehlerList
			a={zaehlerEntry}
			title="Zähler"
			rows={a.then((x) => x.zaehler)}
		/>
		<WalterVertraege title="Verträge" rows={a.then((x) => x.vertraege)} />
		<WalterUmlagen
			a={umlageEntry}
			title="Umlagen"
			rows={a.then((x) => x.umlagen)}
		/>
		<WalterBetriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
		<WalterErhaltungsaufwendungen
			a={erhaltungsaufwendungEntry}
			title="Erhaltungsaufwendungen"
			rows={a.then((x) => x.erhaltungsaufwendungen)}
		/>
	</Accordion>
</WalterGrid>
