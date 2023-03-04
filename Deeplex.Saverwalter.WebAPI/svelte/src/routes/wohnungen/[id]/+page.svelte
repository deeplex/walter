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
	import type { WalterWohnungEntry } from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';

	export let data: PageData;
	const url = `/api/wohnungen/${data.id}`;

	const a: Promise<WalterWohnungEntry> = walter_get(url);
	const entry: Partial<WalterWohnungEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.adresse.anschrift + ' - ' + x.bezeichnung);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterWohnung {a} {entry} />

	<Accordion>
		<WalterWohnungen entry={{}} title="Haus" rows={a.then((x) => x.haus)} />
		<WalterZaehlerList
			entry={{}}
			title="Zähler"
			rows={a.then((x) => x.zaehler)}
		/>
		<WalterVertraege
			entry={{}}
			title="Verträge"
			rows={a.then((x) => x.vertraege)}
		/>
		<WalterUmlagen entry={{}} title="Umlagen" rows={a.then((x) => x.umlagen)} />
		<WalterBetriebskostenrechnungen
			entry={{}}
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
		<WalterErhaltungsaufwendungen
			entry={{}}
			title="Erhaltungsaufwendungen"
			rows={a.then((x) => x.erhaltungsaufwendungen)}
		/>
	</Accordion>
</WalterGrid>
