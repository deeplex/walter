<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		WalterGrid,
		Wohnungen,
		Vertraege,
		Zaehler,
		Erhaltungsaufwendungen,
		Betriebskostenrechnungen,
		Umlagen,
		WalterHeaderDetail,
		WalterWohnung
	} from '$components';
	import type { WohnungEntry } from '$types';
	import { walter_get } from '$services/requests';

	export let data: PageData;
	const url = `/api/wohnungen/${data.id}`;

	const a: Promise<WohnungEntry> = walter_get(url);
	const entry: Partial<WohnungEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => 'TODO - ' + x.bezeichnung);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterWohnung {a} {entry} />

	<Accordion>
		<Wohnungen title="Haus" rows={a.then((x) => x.haus)} />
		<Zaehler title="Zähler" rows={a.then((x) => x.zaehler)} />
		<Vertraege title="Verträge" rows={a.then((x) => x.vertraege)} />
		<Umlagen title="Umlagen" rows={a.then((x) => x.umlagen)} />
		<Betriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
		<Erhaltungsaufwendungen
			title="Erhaltungsaufwendungen"
			rows={a.then((x) => x.erhaltungsaufwendungen)}
		/>
	</Accordion>
</WalterGrid>
