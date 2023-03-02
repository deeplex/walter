<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import { WalterGrid, Wohnungen, WalterHeaderDetail } from '$components';
	import { walter_get } from '$services/utils';
	import type { BetriebskostenrechnungEntry } from '$types';
	import WalterBetriebskostenrechnung from '../../../components/details/WalterBetriebskostenrechnung.svelte';

	export let data: PageData;
	const url = `/api/betriebskostenrechnungen/${data.id}`;

	const a: Promise<BetriebskostenrechnungEntry> = walter_get(url);
	const entry: Partial<BetriebskostenrechnungEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then(
		(x) =>
			x.betreffendesJahr +
			' - TODO - ' +
			x.umlage.text +
			' - TODO - ' +
			x.umlage.text
	);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterBetriebskostenrechnung {a} {entry} />

	<Accordion>
		<Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
	</Accordion>
</WalterGrid>
