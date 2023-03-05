<script lang="ts">
	import { Accordion, Button } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import {
		WalterGrid,
		WalterWohnungen,
		WalterHeaderDetail,
		WalterBetriebskostenrechnung
	} from '$WalterComponents';
	import type { WalterBetriebskostenrechnungEntry } from '$WalterTypes';

	export let data: PageData;
	const url = `/api/betriebskostenrechnungen/${data.id}`;

	const a: Partial<WalterBetriebskostenrechnungEntry> = data.a;
</script>

<WalterHeaderDetail
	{a}
	{url}
	title={a.typ?.text + ' - ' + a.betreffendesJahr + ' - ' + a.umlage?.text}
/>

<WalterGrid>
	<WalterBetriebskostenrechnung {a} />

	<Accordion>
		<WalterWohnungen title="Wohnungen" rows={a.wohnungen || []} />
	</Accordion>

	{#await a then x}
		<Button href={`/umlagen/${x.umlage?.id}`}>Zur Umlage</Button>
	{/await}
</WalterGrid>
