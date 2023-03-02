<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		WalterHeaderDetail,
		WalterGrid,
		Zaehler,
		Zaehlerstaende,
		WalterZaehler
	} from '$components';
	import { walter_get } from '$services/utils';
	import type { ZaehlerEntry } from '$types';

	export let data: PageData;
	const url = `/api/zaehler/${data.id}`;

	const a: Promise<ZaehlerEntry> = walter_get(url);
	const entry: Partial<ZaehlerEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.kennnummer);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterZaehler {a} {entry} />

	<Accordion>
		<Zaehlerstaende title="Zählerstände" rows={a.then((x) => x.staende)} />
		<Zaehler title="Einzelzähler" rows={a.then((x) => x.einzelzaehler)} />
	</Accordion>
</WalterGrid>
