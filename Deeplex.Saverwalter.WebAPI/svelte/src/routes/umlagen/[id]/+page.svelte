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
	import type { WalterUmlageEntry } from '$WalterTypes';

	export let data: PageData;
	const url = `/api/umlagen/${data.id}`;

	const a: Promise<WalterUmlageEntry> = walter_get(url);
	const entry: Partial<WalterUmlageEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.typ.text + ' - ' + x.wohnungenBezeichnung);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterUmlage {a} {entry} />

	<Accordion>
		<WalterWohnungen
			entry={{}}
			title="Wohnungen"
			rows={a.then((x) => x.wohnungen)}
		/>
		<WalterBetriebskostenrechnungen
			entry={{}}
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
	</Accordion>
</WalterGrid>
