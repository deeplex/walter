<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';
	import type { PageData } from './$types';

	import {
		WalterKontakte,
		WalterMieten,
		WalterMietminderungen,
		WalterHeaderDetail,
		WalterGrid,
		WalterVertrag
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterVertragEntry } from '$WalterTypes';

	export let data: PageData;
	const url = `/api/vertraege/${data.id}`;

	const a: Promise<WalterVertragEntry> = walter_get(url);
	const entry: Partial<WalterVertragEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then(
		(x) => x.wohnung.text + ' - ' + x.mieter.map((m) => m.name).join(', ')
	);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterVertrag {a} {entry} />

	<Accordion>
		<WalterKontakte title="Mieter" rows={a.then((x) => x.mieter)} />
		<WalterMieten title="Mieten" rows={a.then((x) => x.mieten)} />
		<WalterMietminderungen
			title="Mietminderungen"
			rows={a.then((x) => x.mietminderungen)}
		/>
	</Accordion>
</WalterGrid>
