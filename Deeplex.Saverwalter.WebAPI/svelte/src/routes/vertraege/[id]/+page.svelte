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
	import type {
		WalterMieteEntry,
		WalterMietminderungEntry,
		WalterVertragEntry
	} from '$WalterTypes';
	import WalterVertragVersionen from '../../../components/lists/WalterVertragVersionen.svelte';
	import { toLocaleIsoString } from '$WalterServices/utils';

	export let data: PageData;
	const url = `/api/vertraege/${data.id}`;

	const a: Promise<WalterVertragEntry> = walter_get(url);
	const entry: Partial<WalterVertragEntry> = {};
	a.then((e) => Object.assign(entry, e));

	const title = a.then(
		(x) => x.wohnung.text + ' - ' + x.mieter.map((m) => m.name).join(', ')
	);

	const mieteEntry: Promise<Partial<WalterMieteEntry>> = a.then((e) => ({
		vertrag: { id: '' + e.id, text: '' },
		zahlungsdatum: toLocaleIsoString(new Date()),
		betrag: e.versionen[e.versionen.length - 1].grundmiete
	}));

	const mietminderungEntry: Promise<Partial<WalterMietminderungEntry>> = a.then(
		(e) => {
			const today = new Date();
			return {
				vertrag: { id: '' + e.id, text: '' },
				beginn: toLocaleIsoString(new Date()),
				ende: toLocaleIsoString(new Date(today.setMonth(today.getMonth() + 1)))
			};
		}
	);
</script>

<WalterHeaderDetail {a} {url} {entry} {title} />

<WalterGrid>
	<WalterVertrag {a} {entry} />

	<Accordion>
		<WalterKontakte title="Mieter" rows={a.then((x) => x.mieter)} />
		<!-- TODO -->
		<WalterVertragVersionen
			title="Versionen"
			rows={a.then((x) => x.versionen)}
		/>
		<WalterMieten
			a={mieteEntry}
			title="Mieten"
			rows={a.then((x) => x.mieten)}
		/>
		<WalterMietminderungen
			a={mietminderungEntry}
			title="Mietminderungen"
			rows={a.then((x) => x.mietminderungen)}
		/>
	</Accordion>
</WalterGrid>
