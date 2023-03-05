<script lang="ts">
	import { Accordion } from 'carbon-components-svelte';

	import type { PageData } from './$types';

	import {
		WalterHeaderDetail,
		WalterGrid,
		WalterZaehler,
		WalterZaehlerstaende,
		WalterZaehlerList,
		WalterZaehlerstand
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type {
		WalterZaehlerEntry,
		WalterZaehlerstandEntry
	} from '$WalterTypes';
	import { toLocaleIsoString } from '$WalterServices/utils';

	export let data: PageData;
	const url = `/api/zaehler/${data.id}`;

	const a: Promise<WalterZaehlerEntry> = walter_get(url);
	const entry: Partial<WalterZaehlerEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then((x) => x.kennnummer);

	const zaehlerstandEntry: Promise<Partial<WalterZaehlerstandEntry>> = a.then(
		(e) => {
			const last = e.staende[e.staende.length - 1] || undefined;
			return {
				zaehler: { id: '' + e.id, text: e.kennnummer },
				datum: toLocaleIsoString(new Date()),
				stand: last.stand || 0,
				einheit: last.einheit
			};
		}
	);

	const einzelzaehlerEntry: Promise<Partial<WalterZaehlerEntry>> = a.then(
		(e) => ({
			adresse: { ...e.adresse },
			typ: e.typ,
			allgemeinZaehler: { id: '' + e.id, text: e.kennnummer }
		})
	);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterZaehler {a} {entry} />

	<Accordion>
		<WalterZaehlerstaende
			a={zaehlerstandEntry}
			title="Zählerstände"
			rows={a.then((x) => x.staende)}
		/>
		<WalterZaehlerList
			a={einzelzaehlerEntry}
			title="Einzelzähler"
			rows={a.then((x) => x.einzelzaehler)}
		/>
	</Accordion>
</WalterGrid>
