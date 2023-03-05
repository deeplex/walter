<script lang="ts">
	import { Accordion, Button } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import {
		WalterGrid,
		WalterWohnungen,
		WalterHeaderDetail,
		WalterBetriebskostenrechnung
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterBetriebskostenrechnungEntry } from '$WalterTypes';

	export let data: PageData;
	const url = `/api/betriebskostenrechnungen/${data.id}`;

	const a: Promise<WalterBetriebskostenrechnungEntry> = walter_get(url);
	const entry: Partial<WalterBetriebskostenrechnungEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then(
		(x) => x.typ.text + ' - ' + x.betreffendesJahr + ' - ' + x.umlage.text
	);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterBetriebskostenrechnung {a} {entry} />

	<Accordion>
		<WalterWohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
	</Accordion>

	{#await a then x}
		<Button href={`/umlagen/${x.umlage.id}`}>Zur Umlage</Button>
	{/await}
</WalterGrid>
