<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		Betriebskostenrechnungen,
		WalterHeaderDetail,
		WalterGrid,
		Wohnungen,
		WalterUmlage
	} from '$components';
	import { walter_get } from '$services/utils';
	import type { UmlageEntry } from '$types';

	export let data: PageData;
	const url = `/api/umlagen/${data.id}`;

	const a: Promise<UmlageEntry> = walter_get(url);
	const entry: Partial<UmlageEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.typ + ' - ' + x.wohnungenBezeichnung);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterUmlage {a} {entry} />

	<Accordion>
		<Wohnungen title="Wohnungen" rows={a.then((x) => x.wohnungen)} />
		<Betriebskostenrechnungen
			title="Betriebskostenrechnungen"
			rows={a.then((x) => x.betriebskostenrechnungen)}
		/>
	</Accordion>
</WalterGrid>
