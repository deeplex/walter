<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import {
		Kontakte,
		Mieten,
		Mietminderungen,
		WalterHeaderDetail,
		WalterGrid,
		WalterVertrag
	} from '$components';
	import { walter_get } from '$services/requests';
	import type { VertragEntry } from '$types';

	export let data: PageData;
	const url = `/api/vertraege/${data.id}`;

	const a: Promise<VertragEntry> = walter_get(url);
	const entry: Partial<VertragEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then(
		(x) => x.wohnung.text + ' - ' + x.mieter.map((m) => m.name).join(', ')
	);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterVertrag {a} {entry} />

	<Accordion>
		<Kontakte title="Mieter" rows={a.then((x) => x.mieter)} />
		<Mieten title="Mieten" rows={a.then((x) => x.mieten)} />
		<Mietminderungen
			title="Mietminderungen"
			rows={a.then((x) => x.mietminderungen)}
		/>
	</Accordion>
</WalterGrid>
