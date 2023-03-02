<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterZaehler,
		WalterZaehlerstaende,
		WalterZaehlerList
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterZaehlerEntry } from '$WalterTypes';

	export let data: PageData;
	const url = `/api/zaehler/${data.id}`;

	const a: Promise<WalterZaehlerEntry> = walter_get(url);
	const entry: Partial<WalterZaehlerEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.kennnummer);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterZaehler {a} {entry} />

	<Accordion>
		<WalterZaehlerstaende
			title="Zählerstände"
			rows={a.then((x) => x.staende)}
		/>
		<WalterZaehlerList
			title="Einzelzähler"
			rows={a.then((x) => x.einzelzaehler)}
		/>
	</Accordion>
</WalterGrid>
