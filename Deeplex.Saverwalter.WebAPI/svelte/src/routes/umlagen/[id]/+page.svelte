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

	const a: Promise<WalterUmlageEntry> = walter_get(url);
	const entry: Partial<WalterUmlageEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.typ.text + ' - ' + x.wohnungenBezeichnung);

	const betriebskostenrechungEntry: Promise<
		Partial<WalterBetriebskostenrechnungEntry>
	> = a.then((e) => {
		const last =
			e.betriebskostenrechnungen[e.betriebskostenrechnungen.length - 1];
		const umlage: WalterSelectionEntry = {
			id: '' + e.id,
			text: e.wohnungenBezeichnung,
			filter: '' + e.typ
		};
		return {
			typ: e.typ,
			umlage: umlage,
			betrag: last.betrag,
			betreffendesJahr: last.betreffendesJahr + 1,
			datum: toLocaleIsoString(new Date())
		} as WalterBetriebskostenrechnungEntry;
	});
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterUmlage {a} {entry} />

	<Accordion>
		<WalterWohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
		<WalterBetriebskostenrechnungen
			a={betriebskostenrechungEntry}
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
	</Accordion>
</WalterGrid>
